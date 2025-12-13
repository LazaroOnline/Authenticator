using System.IO;
using Splat;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Configuration;

namespace Authenticator;

public class Program
{
	public enum AppCommand
	{
		CopyToken
	}

	public readonly static Dictionary<string, string> CommandlineShortKeyMap = new ()
	{
		{ "-M", $"{nameof(AppSettings.MfaGeneratorSecretKey)}" },
	};

	public static bool IsHelpCommand(string[] args)
	{
		return args?.Any(arg =>
			IsCommandArgument(arg, "h") ||
			IsCommandArgument(arg, "help")
		) ?? false;
	}

	public static void DisplayHelp()
	{
		// TODO: make the project able to print to the console, at the moment Console.WriteLine doesn't work with project type WinExe.
		Console.WriteLine($"Authenticator HELP:");
		foreach (var shortCommand in CommandlineShortKeyMap)
		{
			Console.WriteLine($"{shortCommand.Key}  {shortCommand.Value}");
		}
	}

	// Initialization code. Don't use any Avalonia, third-party APIs or any SynchronizationContext-reliant code before AppMain is called:
	// things aren't initialized yet and stuff might break.
	[STAThread]
	public static void Main(string[] args)
	{
		Console.WriteLine($"Starting {nameof(Authenticator)} app...");

		if (IsHelpCommand(args)) {
			DisplayHelp();
			return;
		}

		var configBuilder = new ConfigurationBuilder()
			.SetBasePath(GetExecutingDir())
			.AddJsonFile(AppSettings.FILENAME, optional: true)
			.AddUserSecrets<Program>(optional: true)
			.AddCommandLine(args, CommandlineShortKeyMap);
		var config = configBuilder.Build();

		// Dependency Injection.
		RegisterServices(config);
		var isCopyTokenCommandLineRequest = HasCommandArgument(args, AppCommand.CopyToken);
		if (isCopyTokenCommandLineRequest)
		{
			ExecuteCommandLine(async viewModel => {
				if (string.IsNullOrEmpty(viewModel.MfaGeneratorSecretKey)) {
					FileLogger.Log($"Error: Missing configuration parameter '{nameof(AppSettings.MfaGeneratorSecretKey)}'.");
					return;
				}
				await viewModel.GenerateTokenAndCopyToClipboardCommand();
				FileLogger.Log($"Token: '{viewModel.TokenCode}'.");
			}, nameof(AppCommand.CopyToken)).Wait();
			return;
		}

		BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
	}

	public static async Task ExecuteCommandLine(Func<AuthenticatorViewModel, Task> action, string commandName = "")
	{
		FileLogger.Log($"Running command-line {commandName}");
		var viewModel = Splat.Locator.Current.GetService<AuthenticatorViewModel>();
		if (viewModel == null ) {
			throw new ArgumentNullException($"Failed to find the viewmodel for {nameof(AuthenticatorViewModel)}.");
		}
		try {
			await action(viewModel);
		} catch (Exception ex) {
			FileLogger.Log($"Error: Unhandled exception: " + ex);
		}
		Console.WriteLine(viewModel?.Logs);
		if (!string.IsNullOrWhiteSpace(viewModel?.Logs))
		{
			FileLogger.Log("Logs: " + viewModel.Logs);
		}
		FileLogger.Log($"Finished.");
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.LogToTrace()
			.UseReactiveUI();


	public static string GetExecutingDir()
	{
		return System.AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();
	}

	// https://www.reactiveui.net/docs/handbook/dependency-inversion/
	// https://dev.to/ingvarx/avaloniaui-dependency-injection-4aka
	// Example: https://github.com/rbmkio/radish/blob/master/src/Rbmk.Radish/Program.cs
	// Other ways of DI: https://github.com/egramtel/egram.tel/blob/master/src/Tel.Egram/Program.cs
	public static void RegisterServices(IConfiguration config)
	{
		RegisterServices(Locator.CurrentMutable, Locator.Current, config);
	}

	public static void RegisterServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver, IConfiguration config)
	{
		if (config == null) { throw new ArgumentException($"Null parameter '{nameof(config)}'."); }
		var appSettings = config.Get<AppSettings>() ?? new AppSettings();

		services.Register<AppSettings>(() => config.Get<AppSettings>() ?? new AppSettings());
		services.Register<AppSettingsWriter>(() => new AppSettingsWriter());
		services.Register<AuthenticatorViewModel>(() => new AuthenticatorViewModel());
	}

	public static bool HasCommandArgument(string[] args, AppCommand command)
	{
		return args.Any(arg => IsCommandArgument(arg, command));
	}

	public static bool IsCommandArgument(string arg, AppCommand command)
	{
		return IsCommandArgument(arg, command.ToString());
	}

	public static bool IsCommandArgument(string arg, string command)
	{
		var commandName = command.ToLower();
		var argName = arg.ToLower().TrimStart('-', '/', '\\').Trim();
		return argName == commandName;
	}

}
