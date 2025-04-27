using AutenticationApi.Models;
using AuthApi.Models.DTO;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthApi.Services
{
    public class JWTService
    {
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly SymmetricSecurityKey _key;
        private readonly SigningCredentials _creds;
        private readonly string _jwtSecret;

        public JWTService(IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            _jwtSecret = jwtSettings.GetValue<string>("SecretKey");
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            _creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
            _tokenHandler = new JwtSecurityTokenHandler();
        }


        public string GenerateJwtToken(User user, string audience, TimeSpan expiration)
        {
            var claims = new List<Claim>
            {
             new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
             new Claim(ClaimTypes.Email, user.Email),
             new Claim("UserType", ((int)user.UserType).ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = "RegisterSystem",
                Audience = audience,
                Expires = DateTime.UtcNow.Add(expiration),
                SigningCredentials = _creds
            };

            SecurityToken token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal ValidateToken(string token, string audience)
        {
            try
            {
                TokenValidationParameters validationParameters = CreatevalidationParameters(audience);
                return _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            }
            catch (SecurityTokenExpiredException)
            {
                throw new UnauthorizedAccessException("El token ha expirado.");
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("El token no es válido."+" "+ ex.Message);
            }
        }
      

        private TokenValidationParameters CreatevalidationParameters(string audience) 
        {
            return  new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "RegisterSystem",
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _creds.Key
            };
        }

        
    }
}

