using System.IdentityModel.Tokens.Jwt;

namespace IdentityMinimalAPIs.Services.TokenServices
{
    public class JwtSecurityTokenHandlerFactory
    {
        public JwtSecurityTokenHandler CreateInstance()
        {
            return new JwtSecurityTokenHandler();
        }
    }
}
