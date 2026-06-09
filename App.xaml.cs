using Microsoft.Extensions.Logging;
using Quera.Helpers;

namespace Quera;

public partial class App : Application
{
    private IHost _host = null!;
    public IServiceProvider Services => _host.Services;

    protected override void OnStartup(StartupEventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, ex) =>
            Log.Error($"Unhandled: {ex.ExceptionObject}");
        DispatcherUnhandledException += (_, ex) =>
        {
            Log.Error($"Dispatcher: {ex.Exception}");
            ex.Handled = true;
        };

        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IConfigService, ConfigService>();
                services.AddSingleton<ISingleInstanceService, SingleInstanceService>();
                services.AddSingleton<IFileIndexService, FileIndexService>();
                services.AddSingleton<ISearchService, SearchService>();
                services.AddSingleton<IExecutionService, ExecutionService>();
                services.AddSingleton<IHotkeyService, HotkeyService>();
                services.AddSingleton<ITrayService, TrayService>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<MainWindow>();
            })
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Warning);
                logging.AddDebug();
            })
            .Build();

        var singleInstance = _host.Services.GetRequiredService<ISingleInstanceService>();
        if (!singleInstance.TryAcquire())
        {
            singleInstance.SignalExistingInstance();
            Shutdown();
            return;
        }

        var configService = _host.Services.GetRequiredService<IConfigService>();
        configService.Load();
        var cfg = configService.Current;

        HandleAutoStart(cfg.AutoStart);

        var trayService = _host.Services.GetRequiredService<ITrayService>();
        var window = _host.Services.GetRequiredService<MainWindow>();
        trayService.Create(window);

        window.Show();

        var hotkeyService = _host.Services.GetRequiredService<IHotkeyService>();
        hotkeyService.Initialize(window);
        hotkeyService.Register(cfg.Hotkey);

        var indexService = _host.Services.GetRequiredService<IFileIndexService>();
        _ = Task.Run(async () =>
        {
            try { await indexService.BuildCacheAsync(cfg.SearchPaths, cfg.FileTypes); }
            catch (Exception ex) { Log.Error("Index build failed", ex); }
        });

        singleInstance.StartSignalListener(() =>
        {
            Application.Current.Dispatcher.Invoke(window.ToggleVisibility);
        });

        hotkeyService.HotkeyPressed += () =>
        {
            Application.Current.Dispatcher.Invoke(window.ToggleVisibility);
        };

        if (!cfg.ShowOnStartup)
            window.Hide();
        else
            window.ToggleVisibility();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }

    private static void HandleAutoStart(bool enable)
    {
        var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Run", true);
        if (key == null) return;

        var exePath = Environment.ProcessPath ?? "";

        if (enable)
            key.SetValue("Quera", exePath);
        else
        {
            try { key.DeleteValue("Quera", false); }
            catch { /* key may not exist */ }
        }
    }
}
