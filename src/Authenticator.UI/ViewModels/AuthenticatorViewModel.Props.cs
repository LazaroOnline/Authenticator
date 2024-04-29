namespace Authenticator.ViewModels;

public partial class AuthenticatorViewModel : BaseViewModel
{
	/// <summary>
	/// Token generator secret key of 64 alpha-numeric characters code used to generate the OTPs.
	/// </summary>
	private string _mfaGeneratorSecretKey = "";
	public string MfaGeneratorSecretKey
	{
		get => _mfaGeneratorSecretKey;
		set => this.RaiseAndSetIfChanged(ref _mfaGeneratorSecretKey, value);
	}

	private string _tokenCode = "";
	public string TokenCode
	{
		get => _tokenCode;
		set => this.RaiseAndSetIfChanged(ref _tokenCode, value);
	}

	private bool _isValidMfaGeneratorSecretKey;
	public bool IsValidMfaGeneratorSecretKey
	{
		get => _isValidMfaGeneratorSecretKey;
		set => this.RaiseAndSetIfChanged(ref _isValidMfaGeneratorSecretKey, value);
	}

	private bool _isEnabledGenerateTokenButton;
	public bool IsEnabledGenerateTokenButton
	{
		get => _isEnabledGenerateTokenButton;
		set => this.RaiseAndSetIfChanged(ref _isEnabledGenerateTokenButton, value);
	}

	private bool _hasTokenCode;
	public bool HasTokenCode
	{
		get => _hasTokenCode;
		set => this.RaiseAndSetIfChanged(ref _hasTokenCode, value);
	}

	private DateTime? _tokenExpirationTime;
	public DateTime? TokenExpirationTime
	{
		get => _tokenExpirationTime;
		set => this.RaiseAndSetIfChanged(ref _tokenExpirationTime, value);
	}

	private double? _tokenSecondsToExpire;
	public double? TokenSecondsToExpire
	{
		get => _tokenSecondsToExpire;
		set => this.RaiseAndSetIfChanged(ref _tokenSecondsToExpire, value);
	}

	private double _tokenTimeLeftPercentage;
	public double TokenTimeLeftPercentage
	{
		get => _tokenTimeLeftPercentage;
		set => this.RaiseAndSetIfChanged(ref _tokenTimeLeftPercentage, value);
	}
}
