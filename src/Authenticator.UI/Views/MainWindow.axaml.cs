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
		WindowStateTracker.TryTrackWindow(this);
		this.Closing += (_, _) => { }; // This is just to prevent dotnet trimming from removing the "Closing" event from the Avalonia Window class.
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
