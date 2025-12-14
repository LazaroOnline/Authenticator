using System.Text.Json.Serialization;

namespace Authenticator;

// This "SourceGenerationContext" is used to prevent dotnet trimming from breaking the System.Text.Json.Serialization for the specified class.
// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal partial class SourceGenerationContext : JsonSerializerContext { }

//[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public class AppSettings
{
	public const string FILENAME = "AppSettings.json";

	public string MfaGeneratorSecretKey { get; set; } = "";
	public string? TokenCode { get; set; }
}
