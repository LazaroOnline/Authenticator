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
	}

	public void OpenAboutDialog(object sender, RoutedEventArgs args)
	{
		this.AboutViewDialog.IsVisible = true;
	}
}
