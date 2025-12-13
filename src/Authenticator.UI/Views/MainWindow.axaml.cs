using Avalonia.Markup.Xaml;

namespace Authenticator.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
#if DEBUG
		this.AttachDevTools();
#endif
		WindowStateTracker.TrackWindow(this);
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}

	protected override void OnClosing(WindowClosingEventArgs e)
	{
		base.OnClosing(e);
		var authenticatorForm = this.FindControl<AuthenticatorForm>("AuthenticatorForm");
		var viewModel = (AuthenticatorViewModel?)authenticatorForm?.DataContext;
		viewModel?.SaveConfig();
	}
}
