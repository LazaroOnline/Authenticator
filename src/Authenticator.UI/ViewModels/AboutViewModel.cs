namespace Authenticator.ViewModels;

public class AboutViewModel : BaseViewModel
{
	private const string URL_LICENSE = "https://opensource.org/licenses/MIT";
	private const string URL_GITHUB = "https://github.com/LazaroOnline/Authenticator";

	public string GitVersion { get; set; }

	public AboutViewModel()
	{
		try
		{
			GitVersion = GitVersionService.GetGitVersionAssemblyInfo().ToShortString().TrimEnd('\n').TrimEnd('\r');
		}
		catch
		{
			GitVersion = "";
		}
	}

	public void OpenLinkLicense()
	{
		Util.OpenUrl(URL_LICENSE);
	}

	public void OpenLinkGitHub()
	{
		Util.OpenUrl(URL_GITHUB);
	}
}
