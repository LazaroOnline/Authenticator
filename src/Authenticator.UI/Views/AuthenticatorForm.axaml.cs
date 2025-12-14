using Avalonia.Interactivity;

namespace Authenticator.Views;

public partial class AuthenticatorForm : UserControl
{
	public AuthenticatorForm()
	{
		InitializeComponent();
		if (TokenTextBox != null)
		{
			TokenTextBox.AttachedToVisualTree += (s, e) => TokenTextBox.Focus();
		}
		this.Unloaded += (_, _) => OnUnloadedSaveConfig();
	}

	public void OpenAboutDialog(object sender, RoutedEventArgs args)
	{
		this.AboutViewDialog.IsVisible = true;
	}

	public void OnUnloadedSaveConfig()
	{
		var viewModel = (AuthenticatorViewModel?)this.DataContext;
		viewModel?.SaveConfig();
	}
}
