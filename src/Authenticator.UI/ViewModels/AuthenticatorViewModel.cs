using Splat;

namespace Authenticator.ViewModels;

public partial class AuthenticatorViewModel : BaseViewModel
{
	private readonly AppSettingsWriter _appSettingsWriter;

	// Constructor required by the designer tools.
	public AuthenticatorViewModel()
		: this(null, null) // Call the other constructor
	{
		System.Diagnostics.Debug.WriteLine($"Default constructor used for {nameof(AuthenticatorViewModel)}.");
	}

	public AuthenticatorViewModel(
		 AppSettingsWriter? appSettingsWriter
		,AppSettings? appSettings
	)
	{
		_appSettingsWriter = appSettingsWriter ?? Splat.Locator.Current.GetService<AppSettingsWriter>() ?? new AppSettingsWriter();
		appSettings = appSettings ?? Splat.Locator.Current.GetService<AppSettings>();

		MfaGeneratorSecretKey = appSettings?.MfaGeneratorSecretKey ?? "";
		TokenCode = appSettings?.TokenCode ?? "";

		this.WhenAnyValue(x => x.MfaGeneratorSecretKey).Subscribe(x => {
			this.IsValidMfaGeneratorSecretKey = GetIsValidMfaGeneratorSecretKey();
		});

		this.WhenAnyValue(x => x.MfaGeneratorSecretKey, x => x.IsLoading).Subscribe(x => {
			this.IsEnabledGenerateTokenButton = GetIsEnabledGenerateTokenButton();
		});

		this.WhenAnyValue(x => x.TokenCode).Subscribe(x => {
			this.HasTokenCode = !GetIsTokenCodeEmpty();
			ClearTokenExpiration();
		});
	}

	public bool GetIsEnabledGenerateTokenButton() =>
		!IsLoading && IsValidMfaGeneratorSecretKey;

	public bool GetIsTokenCodeEmpty() =>
		string.IsNullOrWhiteSpace(TokenCode);


	public void TokenCodeClearCommand()
	{
		TokenCode = "";
	}

	public async Task GenerateTokenCommand()
	{
		// This command is fast, it could run synchronously if required.
		// WithExceptionLogging(async () => {
		await ExecuteAsyncWithLoadingAndExceptionLogging(() => {
			_ = SetGeneratedTokenFromAuthenticatorKey();
			if (!string.IsNullOrEmpty(TokenCode))
			{
				LogsColorSetInfo();
				Logs = $"TOKEN-CODE GENERATED: {TokenCode}    ({DateTime.Now.ToString(DATE_FORMAT)})";
			}
		});
	}

	public async Task CopyTokenCodeCommand()
	{
		// This command is fast, it could run synchronously if required.
		// WithExceptionLogging(async () => {
		await ExecuteAsyncWithLoadingAndExceptionLogging(async () => {
			await Clipboard.SetTextAsync(TokenCode);
		});
	}

	public async Task GenerateTokenAndCopyToClipboardCommand()
	{
		await SetGeneratedTokenFromAuthenticatorKey(false);
		var isValidToken = !string.IsNullOrWhiteSpace(TokenCode);
		if (isValidToken) {
			// https://docs.avaloniaui.net/docs/input/clipboard
			await Clipboard.SetTextAsync(TokenCode);
		}
		else
		{
			// Show the errors from the logs:
			await Clipboard.SetTextAsync(Logs);
		}
	}

	public async Task SetGeneratedTokenFromAuthenticatorKey(bool keepRegeneratingTokens = false)
	{
		do
		{
			TokenCode = GenerateTokenFromAuthenticatorKey();
			SetTokenExpiration();
			var refreshExpirationTask = RefreshTokenExpirationInfoUntilExpires();
			if (keepRegeneratingTokens)
			{
				await refreshExpirationTask;
			}
		}
		while (keepRegeneratingTokens);
	}

	public string GenerateTokenFromAuthenticatorKey()
	{
		return GenerateTokenFromAuthenticatorKey(MfaGeneratorSecretKey);
	}

	public string GenerateTokenFromAuthenticatorKey(string mfaGeneratorSecretKey)
	{
		var validationErrors = ValidateMfaGeneratorSecretKey(mfaGeneratorSecretKey);
		if (validationErrors.Any())
		{
			LogsColorSetWarning();
			Logs = "Validation error:\r\n" + string.Join("\r\n", validationErrors);
			return "";
		}
		var authenticator = new TwoStepsAuthenticator.TimeAuthenticator();
		var tokenCode = authenticator.GetCode(mfaGeneratorSecretKey);
		return tokenCode;
	}

	public void SetTokenExpiration()
	{
		this.TokenExpirationTime = GetNextTokenExpirationTime();
	}

	public async Task RefreshTokenExpirationInfoUntilExpires()
	{
		do
		{
			RefreshTokenExpirationInfo();
			if (TokenSecondsToExpire != null)
			{
				await Task.Delay(1000);
			}
		}
		while (TokenSecondsToExpire != null);
		Console.WriteLine("Refresh ended");
	}

	public void RefreshTokenExpirationInfo()
	{
		var utcNow = DateTime.UtcNow;
		if (TokenExpirationTime != null && TokenExpirationTime > utcNow)
		{
			this.TokenSecondsToExpire = (TokenExpirationTime.Value - utcNow).Seconds;
			this.TokenTimeLeftPercentage = 100 * (TokenSecondsToExpire.Value / 30);
		}
		else
		{
			ClearTokenExpiration();
		}
	}

	public void ClearTokenExpiration()
	{
		this.TokenExpirationTime = null;
		this.TokenSecondsToExpire = null;
		this.TokenTimeLeftPercentage = 0;
	}

	public bool GetIsValidMfaGeneratorSecretKey() => !ValidateMfaGeneratorSecretKey().Any();

	public IList<string> ValidateMfaGeneratorSecretKey() => ValidateMfaGeneratorSecretKey(MfaGeneratorSecretKey);

	public IList<string> ValidateMfaGeneratorSecretKey(string? mfaSecretKey)
	{
		var errors = new List<string>();

		var fieldName = "Authenticator secret key";

		if (string.IsNullOrWhiteSpace(mfaSecretKey))
		{
			return [$"'{fieldName}' is empty."];
		}

		try
		{
			// Test the Base32 encoder required during token generation:
			// https://github.com/glacasa/TwoStepsAuthenticator/blob/main/TwoStepsAuthenticator/Authenticator.cs#L10
			var key = TwoStepsAuthenticator.Base32Encoding.ToBytes(mfaSecretKey);
		}
		catch (ArgumentException ex)
		{
			if (ex.Message.Contains("Character is not a Base32 character"))
			{
				errors.Add($"Invalid '{fieldName}'. It should be a Base32 set of characters.");
			}
		}
		catch (Exception ex)
		{
			errors.Add($"Invalid '{fieldName}'. Failed converting to Base32.\r\n{ex.GetType()}: {ex.Message}");
		}
		return errors;
	}

	public IList<string> ValidateToken() => ValidateToken(TokenCode);

	public IList<string> ValidateToken(string? token)
	{
		var fieldName = "TokenCode";
		if (string.IsNullOrWhiteSpace(token))
		{
			return [$"{fieldName} is empty."];
		}
		return [];
	}

	public IList<string> ValidateUpdateCredentialsCommand()
	{
		var errors = new List<string>();

		var hasEmptyToken = string.IsNullOrWhiteSpace(TokenCode);

		if (hasEmptyToken)
		{
			var errorsInMfaGeneratorSecretKey = ValidateMfaGeneratorSecretKey();
			errors.AddRange(errorsInMfaGeneratorSecretKey);
		}
		else
		{
			var errorsInTokenCode = ValidateToken();
			errors.AddRange(errorsInTokenCode);
		}
		return errors;
	}

	public DateTime GetNextTokenExpirationTime()
	{
		var now = DateTime.UtcNow;
		var isMinutesFirstHalf = now.Second < 30;
		if (isMinutesFirstHalf)
		{
			return now.AddSeconds(30 - now.Second);
		}
		else
		{
			return now.AddSeconds(60 - now.Second);
		}
	}

	public void SaveConfig()
	{
		var newAppSettings = new AppSettings()
		{
			MfaGeneratorSecretKey = MfaGeneratorSecretKey,
		};
		_appSettingsWriter.Save(newAppSettings);
	}
}
