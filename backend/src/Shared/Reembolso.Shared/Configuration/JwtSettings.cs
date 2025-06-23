namespace Reembolso.Shared.Configuration;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "ReembolsoAPI";
    public string Audience { get; set; } = "ReembolsoClients";
    public int ExpirationInMinutes { get; set; } = 60;
    public int RefreshTokenExpirationInDays { get; set; } = 7;
}