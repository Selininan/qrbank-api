using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using QrBankApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace QrBankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;

        public AuthController(IConfiguration config, IMemoryCache cache)
        {
            _config = config;
            _cache = cache;
        }

        [HttpPost("token")]
        [AllowAnonymous]
        public IActionResult GetToken([FromBody] LoginRequest request)
        {
            var staticUser = _config["Auth:Username"];
            var staticPass = _config["Auth:Password"];

            if (request is null)
                return BadRequest(new { Message = "Geçersiz istek." });

            if (request.Username != staticUser || request.Password != staticPass)
                return Unauthorized(new { Message = "Kullanıcı adı veya şifre hatalı." });

            // Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim("user_id", "42"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("ip", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"),
                new Claim("custom_format", "QR-APP-2025")
            };

            // Access token
            var token = GenerateJwtToken(claims);

            // Refresh token
            var refreshToken = Guid.NewGuid().ToString("N");

            // Cache
            var expireMinutes = int.TryParse(_config["Jwt:ExpireMinutes"], out var m) ? m : 60;
            _cache.Set($"token:{request.Username}", token, TimeSpan.FromMinutes(expireMinutes));
            _cache.Set($"refresh:{request.Username}", refreshToken, TimeSpan.FromHours(2));

            return Ok(new
            {
                token,
                refreshToken,
                expiresInMinutes = expireMinutes
            });
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public IActionResult RefreshToken([FromBody] RefreshRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { Message = "Geçersiz istek." });

            var staticUser = _config["Auth:Username"];
            var cachedRefresh = _cache.Get<string>($"refresh:{staticUser}");

            if (cachedRefresh == null || cachedRefresh != request.RefreshToken)
                return Unauthorized(new { Message = "Refresh token geçersiz veya süresi dolmuş." });

            // Yeni token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, staticUser),
                new Claim("user_id", "42"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("ip", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"),
                new Claim("custom_format", "QR-APP-2025")
            };

            var newToken = GenerateJwtToken(claims);
            _cache.Set($"token:{staticUser}", newToken, TimeSpan.FromMinutes(60));

            return Ok(new { token = newToken });
        }

        private string GenerateJwtToken(IEnumerable<Claim> claims)
        {
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key missing"));
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var expireMinutes = int.TryParse(_config["Jwt:ExpireMinutes"], out var m) ? m : 60;

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
