using Microsoft.IdentityModel.Tokens;
using System.Text;

public static class TokenValidationHelper
{
    public static TokenValidationParameters GetTokenValidationParameters(IConfiguration config)
    {
        var secretKey = config["Jwt:Secret"];
        var issuer = config["Jwt:Issuer"];
        var audience = config["Jwt:Audience"];
        var key = Encoding.UTF8.GetBytes(secretKey!);

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            // ClockSkew = TimeSpan.Zero
        };
    }
}