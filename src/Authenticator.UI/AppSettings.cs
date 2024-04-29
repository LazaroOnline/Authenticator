namespace Authenticator;

public class AppSettings
{
	public const string FILENAME = "AppSettings.json";

	public string MfaGeneratorSecretKey { get; set; } = "";
	public string? TokenCode { get; set; }
}
