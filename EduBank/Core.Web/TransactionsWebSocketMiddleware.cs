using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common;
using Core.Application.Services.Implementations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using Core.Application.Services.Interfaces;

namespace Core.Web
{
    public class TransactionsWebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TransactionsWebSocketConnectionManager _manager;
        private readonly ILogger<TransactionsWebSocketMiddleware> _logger;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly string _authority;
        private readonly string _audience;

        private readonly IAccountService _accountService;


        // Add these fields to the class (if not already present)
        private static IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
        private static readonly object _lock = new object();

        public TransactionsWebSocketMiddleware(
            RequestDelegate next, 
            TransactionsWebSocketConnectionManager manager,
            ILogger<TransactionsWebSocketMiddleware> logger, 
            TokenValidationParameters parameters,
            string authority,
            string audience,
            IAccountService accountService)
        {
            _next = next;
            _manager = manager;
            _logger = logger;
            _tokenValidationParameters = parameters;
            _authority = authority;
            _audience = audience;
            _accountService = accountService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/wstransactions" && context.WebSockets.IsWebSocketRequest)
            {
                if(_tokenValidationParameters == null)
                {
                    context.Response.StatusCode = 500;
                    return;
                }

                // Извлекаем токен
                string token = context.Request.Query["token"];
                if (string.IsNullOrEmpty(token))
                {
                    context.Response.StatusCode = 401;
                    return;
                }

                string? mode = context.Request.Query["fmt"];

                _logger.LogInformation("Using WS token: " + token );

                // Аутентифицируем
                var user = await AuthenticateTokenAsync(token);
                if (user == null)
                {
                    context.Response.StatusCode = 403;
                    return;
                }

                context.User = user;

                var id = context!.GetUserId()!;
                var isManager = context!.User.IsInRole("Employee");
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                _manager.AddSocket(id.Value, isManager, webSocket, (mode ?? "default") == "display");

                /* Оповещаем всех о новом пользователе (опционально)
                var connectMessage = new { type = "system", content = $"User {connectionId} connected" };
                await _manager.BroadcastMessageAsync(JsonSerializer.Serialize(connectMessage));
                */
                await HandleWebSocketCommunication(webSocket, id.Value);
            }
            else
            {
                await _next(context);
            }
        }

        private async Task HandleWebSocketCommunication(WebSocket webSocket, Guid connectionId)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _manager.RemoveSocket(connectionId);
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogInformation($"Got: {receivedMessage} from {connectionId}");
                        string[] tokens = receivedMessage.Split(" ");

                        if(tokens.Length >= 2 && tokens[0] == "get")
                        {
                            var id = Guid.Parse(tokens[1]);

                            var from = tokens.Length >= 3 ? DateTime.Parse(tokens[2]) : DateTime.MinValue;
                            var to = tokens.Length >= 4 ? DateTime.Parse(tokens[3]) : DateTime.MaxValue;
                            _logger.LogInformation($"Got get cmd from {connectionId}: {id}, {from}, {to}");

                            if (_manager.IsDisplayMode(connectionId))
                            {
                                var transactions = await _accountService.GetAccountTransactionsForDisplayAsync(id, from, to, connectionId);

                                foreach (var transaction in transactions)
                                {
                                    await _manager.SendToClientAsync(connectionId, _manager.Serialize(transaction));
                                }
                            }
                            else
                            {
                                var transactions = await _accountService.GetAccountTransactionsAsync(id, from, to, null);

                                foreach (var transaction in transactions)
                                {
                                    await _manager.SendToClientAsync(connectionId, _manager.Serialize(transaction));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                await _manager.RemoveSocket(connectionId);
            }
        }

        private async Task<ClaimsPrincipal?> AuthenticateTokenAsync(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var kid = jwtToken.Header.Kid;

                _logger.LogInformation($"Validating token with kid: {kid ?? "null"}");

                // Get the configuration (cached) from the authority's metadata endpoint
                var config = await GetOpenIdConnectConfigurationAsync();

                // Build the validation parameters
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = _tokenValidationParameters.ValidateIssuer,
                    ValidIssuer = _tokenValidationParameters.ValidIssuer,
                    ValidateAudience = _tokenValidationParameters.ValidateAudience,
                    ValidAudience = _audience,
                    ValidateLifetime = _tokenValidationParameters.ValidateLifetime,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = _tokenValidationParameters.ClockSkew,
                    IssuerSigningKeys = config.JsonWebKeySet?.GetSigningKeys() ?? Enumerable.Empty<SecurityKey>()
                };

                var principal = handler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Authentication failed");
                return null;
            }
        }

        private async Task<OpenIdConnectConfiguration> GetOpenIdConnectConfigurationAsync()
        {
            // Initialize the configuration manager once
            if (_configurationManager == null)
            {
                lock (_lock)
                {
                    if (_configurationManager == null)
                    {
                        var metadataAddress = $"{_authority}/.well-known/openid-configuration";
                        var documentRetriever = new HttpDocumentRetriever { RequireHttps = false };
                        _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                            metadataAddress,
                            new OpenIdConnectConfigurationRetriever(),
                            documentRetriever
                        );
                    }
                }
            }

            return await _configurationManager.GetConfigurationAsync(CancellationToken.None);
        }

        private async Task<RSA?> LoadSigningKeysFromJwksUriAsync(string jwksUri)
        {
            if (string.IsNullOrEmpty(jwksUri))
                return null;

            var retriever = new HttpDocumentRetriever { RequireHttps = false };
            var jwksJson = await retriever.GetDocumentAsync(jwksUri, CancellationToken.None);
            //_logger.LogInformation("got jwks json: " + jwksJson);
            return GetSigningKeyFromJson(jwksJson);
        }

        public class JwtKeyInfo
        {
            [JsonPropertyName("kty")]
            public string Kty { get; set; }
            [JsonPropertyName("use")]
            public string Use { get; set; }
            [JsonPropertyName("e")]
            public string E { get; set; }
            [JsonPropertyName("n")]
            public string N { get; set; }
            [JsonPropertyName("alg")]
            public string Alg { get; set; }
        }

        public class JwkSet
        {
            [JsonPropertyName("keys")]
            public JwtKeyInfo[] Keys { get; set; }
        }

        public static RSA GetSigningKeyFromJson(string json)
        {
            var jwkSet = JsonSerializer.Deserialize<JwkSet>(json);
            if (jwkSet?.Keys == null || jwkSet.Keys.Length == 0)
                throw new InvalidOperationException("No keys found in JSON");

            // Предполагаем, что нужен первый ключ
            var keyInfo = jwkSet.Keys[0];

            if (keyInfo.Kty != "RSA")
                throw new NotSupportedException("Only RSA keys are supported");

            byte[] modulus = FromBase64Url(keyInfo.N);
            byte[] exponent = FromBase64Url(keyInfo.E);

            return CreateRsaPublicKey(modulus, exponent);
        }


        static RSA CreateRsaPublicKey(byte[] modulus, byte[] exponent)
        {
            var rsa = RSA.Create();
            var parameters = new RSAParameters
            {
                Modulus = modulus,
                Exponent = exponent
            };
            rsa.ImportParameters(parameters);
            return rsa;
        }
        static byte[] FromBase64Url(string base64Url)
        {
            string base64 = base64Url
                .Replace('-', '+')
                .Replace('_', '/');
            // Добавляем padding, если нужно
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}