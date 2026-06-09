namespace Quera.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IConfigService _configService;
    private readonly ISearchService _searchService;
    private readonly IExecutionService _executionService;
    private readonly IFileIndexService _fileIndexService;
    private CancellationTokenSource? _searchCts;

    public MainViewModel(IConfigService configService, ISearchService searchService, IExecutionService executionService, IFileIndexService fileIndexService)
    {
        _configService = configService;
        _searchService = searchService;
        _executionService = executionService;
        _fileIndexService = fileIndexService;
    }

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private ObservableCollection<SearchResult> _results = new();

    [ObservableProperty]
    private int _selectedIndex;

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _isVisible;

    public bool HasSearchText => !string.IsNullOrWhiteSpace(SearchText);

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(HasSearchText));
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        Task.Delay(80, token).ContinueWith(_ =>
        {
            if (!token.IsCancellationRequested)
                System.Windows.Application.Current.Dispatcher.Invoke(() => DoSearch());
        }, token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
    }

    private void DoSearch()
    {
        if (!_fileIndexService.CacheBuilt)
        {
            Results.Clear();
            SelectedIndex = 0;
            TotalCount = 0;
            StatusText = "正在索引...";
            return;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Results.Clear();
            SelectedIndex = 0;
            TotalCount = 0;
            StatusText = "就绪";
            return;
        }

        var container = _searchService.Search(SearchText, _configService.Current, _configService.Current.MaxResults);

        Results.Clear();
        foreach (var item in container.Items)
            Results.Add(item);

        TotalCount = container.TotalCount;
        SelectedIndex = 0;

        if (container.TotalCount > container.Items.Count)
            StatusText = $"{container.Items.Count}/{container.TotalCount} 个结果";
        else
            StatusText = $"{container.Items.Count} 个结果";
    }

    [RelayCommand]
    private void SelectNext()
    {
        if (Results.Count > 0)
            SelectedIndex = (SelectedIndex + 1) % Results.Count;
    }

    [RelayCommand]
    private void SelectPrevious()
    {
        if (Results.Count > 0)
            SelectedIndex = (SelectedIndex - 1 + Results.Count) % Results.Count;
    }

    public string? ExecuteSelected()
    {
        if (SelectedIndex < 0 || SelectedIndex >= Results.Count) return null;
        var item = Results[SelectedIndex];
        return _executionService.Execute(item);
    }

    [RelayCommand]
    private void Hide()
    {
        IsVisible = false;
    }

    [RelayCommand]
    private void OpenConfig()
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
        Process.Start("notepad.exe", configPath);
    }

    [RelayCommand]
    private void ReloadConfig()
    {
        _configService.Reload();
        var cfg = _configService.Current;
        _ = Task.Run(async () =>
        {
            var indexService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IFileIndexService>();
            await indexService.BuildCacheAsync(cfg.SearchPaths, cfg.FileTypes);
        });
    }

    public void ResetSearch()
    {
        SearchText = "";
        Results.Clear();
        SelectedIndex = 0;
        TotalCount = 0;
        StatusText = "就绪";
    }
}

public static class AppServices
{
    public static IServiceProvider Services => ((App)System.Windows.Application.Current).Services;
}
