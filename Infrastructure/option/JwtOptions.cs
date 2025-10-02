using System;

namespace Infrastructure.option;


public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string AccessKey { get; set; } = "";   // Base64 32 bytes (256-bit)
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}