using Duende.IdentityServer.Models;

public static class Config
{
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
        new ApiScope("account_api", "Account API"),
        new ApiScope("credit_api", "Credit API"),
        new ApiScope("options_api", "User Options API"),
        new ApiScope("currency_api", "Currency API")
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
        new ApiResource("account_api", "Account API") { Scopes = { "account_api" } },
        new ApiResource("credit_api", "Credit API") { Scopes = { "credit_api" } },
        new ApiResource("currency_api", "Currency API") { Scopes = { "currency_api" } },
        new ApiResource("options_api", "User Options API") { Scopes = { "options_api" } }
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
        var authority = auth["InternalAuthority"] ?? "http://localhost:5000";
        var swaggerUrl = auth["SwaggerUrl"] ?? "http://localhost:5000";

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
                RedirectUris = { "http://37.21.130.4:5001/swagger/oauth2-redirect.html" },
                AllowedCorsOrigins = { "http://37.21.130.4:5001" },
                AllowedScopes = { "openid", "profile", "account_api" },
                AllowAccessTokensViaBrowser = true
            },

            new Client
            {
                ClientId = "credit_service_swagger",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = false,
                RequireClientSecret = false,
                RedirectUris = { "http://37.21.130.4:5002/swagger/oauth2-redirect.html" },
                AllowedCorsOrigins = { "http://37.21.130.4:5002" },
                AllowedScopes = { "openid", "profile", "credit_api" },
                AllowAccessTokensViaBrowser = true
            },


            new Client
            {
                ClientId = "currency_service_swagger",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = false,
                RequireClientSecret = false,
                RedirectUris = { "http://37.21.130.4:5003/swagger/oauth2-redirect.html" },
                AllowedCorsOrigins = { "http://37.21.130.4:5003" },
                AllowedScopes = { "openid", "profile", "currency_api" },
                AllowAccessTokensViaBrowser = true
            },

            new Client
            {
                ClientId = "options_service_swagger",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = false,
                RequireClientSecret = false,
                RedirectUris = { "http://37.21.130.4:5004/swagger/oauth2-redirect.html" },
                AllowedCorsOrigins = { "http://37.21.130.4:5004" },
                AllowedScopes = { "openid", "profile", "options_api" },
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
                AllowedScopes = { "openid", "profile", "account_api", "credit_api", "currency_api", "options_api" }
            },
            new Client
            {
                ClientId = auth["MobileClientId"] ?? "mobile_client",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RequireConsent = false,
                RedirectUris = { "bankclient://auth" },
                AllowedScopes = { "openid", "profile", "account_api", "credit_api", "currency_api", "options_api" }
            },
            new Client
            {
                ClientId = "internal_service",
                ClientSecrets = { new Secret("500cigaretts".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "account_api", "credit_api", "currency_api" }
            },

            new Client
            {
                ClientId = "monitoring_web",
                ClientSecrets = { new Secret("monitoring_secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = false,
                RequireConsent = false,
                RedirectUris = { "http://37.21.130.4:5005/swagger/oauth2-redirect.html" },
                AllowedCorsOrigins = { "http://37.21.130.4:5005" },
                AllowedScopes = { "openid", "profile", "account_api" }
            }
        };
    }
}