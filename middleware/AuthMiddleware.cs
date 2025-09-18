using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;

namespace QrBankApi.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public AuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _secretKey = configuration["Jwt:Key"] ?? throw new Exception("Jwt key not found!");
            _issuer = configuration["Jwt:Issuer"] ?? throw new Exception("Jwt issuer not found!");
            _audience = configuration["Jwt:Audience"] ?? throw new Exception("Jwt audience not found!");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.UTF8.GetBytes(_secretKey);

                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = _issuer,
                        ValidateAudience = true,
                        ValidAudience = _audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero // token süresini birebir uygular
                    }, out SecurityToken validatedToken);

                    // Token geçerliyse context'e ekleyebilirsin
                    context.Items["User"] = ((JwtSecurityToken)validatedToken).Payload;
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync($"Geçersiz token: {ex.Message}");
                    return;
                }
            }

            await _next(context);
        }
    }

    // Extension method
    public static class AuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthMiddleware>();
        }
    }
}
