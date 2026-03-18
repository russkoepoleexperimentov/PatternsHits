using Duende.IdentityServer.Models;

public static class Config
{
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
        new ApiScope("account_api", "Account API"),
        new ApiScope("credit_api", "Credit API")
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
        new ApiResource("account_api", "Account API") { Scopes = { "account_api" } },
        new ApiResource("credit_api", "Credit API") { Scopes = { "credit_api" } }
        };

    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };

    public static IEnumerable<Client> GetClients(IConfiguration config)
    {
        var auth = config.GetSection("Auth");
        var authority = auth["InternalAuthority"] ?? "http://localhost:2280";
        var swaggerUrl = auth["SwaggerUrl"] ?? "http://localhost:2280";

        return new List<Client>
        {
            new Client
            {
                ClientId = auth["SwaggerClientId"] ?? "swagger",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = false,
                RequireClientSecret = false,
                RequireConsent = false,

                RedirectUris = { $"{swaggerUrl}/swagger/oauth2-redirect.html" },
                AllowedCorsOrigins = { swaggerUrl },

                AllowedScopes = { "openid", "profile", "account_api" },
                AllowAccessTokensViaBrowser = true
            },

            new Client
            {
                ClientId = "account_service_swagger",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = false,
                RequireClientSecret = false,
                RedirectUris = { "http://localhost:2281/swagger/oauth2-redirect.html" },
                AllowedCorsOrigins = { "http://localhost:2281" },
                AllowedScopes = { "openid", "profile", "account_api" },
                AllowAccessTokensViaBrowser = true
            },

            new Client
            {
                ClientId = "credit_service_swagger",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = false,
                RequireClientSecret = false,
                RedirectUris = { "http://localhost:5001/swagger/oauth2-redirect.html" },
                AllowedCorsOrigins = { "http://localhost:5001" },
                AllowedScopes = { "openid", "profile", "credit_api" },
                AllowAccessTokensViaBrowser = true
            },
            new Client
            {
                ClientId = auth["WebClientId"] ?? "web_client",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RequireConsent = false,
                RedirectUris = { $"{auth["WebUrl"]}/signin-oidc" },
                PostLogoutRedirectUris = { $"{auth["WebUrl"]}/signout-callback-oidc" },
                AllowedCorsOrigins = { auth["WebUrl"] },
                AllowedScopes = { "openid", "profile", "account_api" }
            },
            new Client
            {
                ClientId = auth["MobileClientId"] ?? "mobile_client",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RequireConsent = false,
                RedirectUris = { "myapp://callback" },
                AllowedScopes = { "openid", "profile", "account_api" }
            },
            new Client
            {
                ClientId = "internal_service",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "account_api" }
            }
        };
    }
}