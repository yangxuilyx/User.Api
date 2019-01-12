using System.Collections.Generic;
using IdentityServer4.Models;

namespace User.Identity
{
    public class Config
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new ApiResource[]
            {
                new ApiResource("api1","Api Resource"),
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new Client[]
            {
                new Client()
                    {
                        ClientId = "client",
                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        // 用于认证的密码
                        ClientSecrets =
                        {
                        new Secret("secret".Sha256())
                    },
                    // 客户端有权访问的范围（Scopes）
                    AllowedScopes = { "api1" }
                },
                new Client()
                {
                    ClientId = "phoneClient",
                    AllowedGrantTypes = new List<string>()
                    {
                       "sms_auth_code"
                    },
                    // 用于认证的密码
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    // 客户端有权访问的范围（Scopes）
                    AllowedScopes = { "api1" },
                    AllowOfflineAccess = true,
                },
            };
        }
    }
}