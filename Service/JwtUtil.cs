using System;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace QuizApp.Services
{
    public static class JwtUtil
    {
        private const string SecretKey = "mySecretKey"; // In production, store it securely
        private static readonly SymmetricSecurityKey SecurityKey = new(Encoding.UTF8.GetBytes(SecretKey));
        private static readonly JwtSecurityTokenHandler TokenHandler = new();

        public static string GenerateToken(string username, string role, string quizIndex)
        {
            var claims = new[]
            {
                new Claim("sub", username),
                new Claim("role", role),
                new Claim("quizIndex", quizIndex)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256)
            };

            var token = TokenHandler.CreateToken(tokenDescriptor);
            return TokenHandler.WriteToken(token);
        }

        public static ClaimsPrincipal? VerifyToken(string token)
        {
            try
            {
                var principal = TokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = SecurityKey
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}