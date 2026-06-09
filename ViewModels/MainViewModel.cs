using System.Windows.Threading;

namespace Quera.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IConfigService _configService;
    private readonly ISearchService _searchService;
    private readonly IExecutionService _executionService;
    private readonly IFileIndexService _fileIndexService;
    private readonly DispatcherTimer _debounceTimer;
    private List<SearchResult> _allResults = new();
    private int _page;
    private int _pageSize;

    public MainViewModel(IConfigService configService, ISearchService searchService, IExecutionService executionService, IFileIndexService fileIndexService)
    {
        _configService = configService;
        _searchService = searchService;
        _executionService = executionService;
        _fileIndexService = fileIndexService;
        _pageSize = _configService.Current.UI.MaxVisibleItems;

        _debounceTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(60), DispatcherPriority.Normal, OnDebounceTick, Dispatcher.CurrentDispatcher);
        _debounceTimer.Stop();
    }

    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private ObservableCollection<SearchResult> _results = new();
    [ObservableProperty] private int _selectedIndex;
    [ObservableProperty] private string _statusText = "就绪";
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private bool _isVisible;

    public bool HasSearchText => !string.IsNullOrWhiteSpace(SearchText);

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(HasSearchText));
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    private void OnDebounceTick(object? sender, EventArgs e)
    {
        _debounceTimer.Stop();
        DoSearch();
    }

    private void DoSearch()
    {
        try
        {
            if (!_fileIndexService.CacheBuilt)
            {
                ClearAll();
                StatusText = "正在索引...";
                return;
            }

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ClearAll();
                StatusText = "就绪";
                return;
            }

            var container = _searchService.Search(SearchText, _configService.Current, _configService.Current.MaxResults);
            _allResults = container.Items;
            _page = 0;
            TotalCount = container.TotalCount;
            ShowPage();
        }
        catch (Exception ex)
        {
            Log.Error("DoSearch failed", ex);
        }
    }

    private void ShowPage()
    {
        var skip = _page * _pageSize;
        var items = _allResults.Skip(skip).Take(_pageSize).ToList();

        Results.Clear();
        foreach (var item in items) Results.Add(item);

        SelectedIndex = 0;
        var totalPages = (_allResults.Count + _pageSize - 1) / _pageSize;
        StatusText = _allResults.Count == 0
            ? "无结果"
            : $"{_page + 1}/{totalPages} 页 ({TotalCount} 条)";
    }

    [RelayCommand]
    private void SelectNext()
    {
        if (Results.Count == 0) return;

        if (SelectedIndex < Results.Count - 1)
        {
            SelectedIndex++;
        }
        else
        {
            var totalPages = (_allResults.Count + _pageSize - 1) / _pageSize;
            _page = (_page + 1) % totalPages;
            ShowPage();
        }
    }

    [RelayCommand]
    private void SelectPrevious()
    {
        if (Results.Count == 0) return;

        if (SelectedIndex > 0)
        {
            SelectedIndex--;
        }
        else
        {
            var totalPages = (_allResults.Count + _pageSize - 1) / _pageSize;
            _page = (_page - 1 + totalPages) % totalPages;
            ShowPage();
            SelectedIndex = Results.Count - 1;
        }
    }

    public void NextPage()
    {
        if (_allResults.Count == 0) return;
        var totalPages = (_allResults.Count + _pageSize - 1) / _pageSize;
        _page = (_page + 1) % totalPages;
        ShowPage();
    }

    public void PrevPage()
    {
        if (_allResults.Count == 0) return;
        var totalPages = (_allResults.Count + _pageSize - 1) / _pageSize;
        _page = (_page - 1 + totalPages) % totalPages;
        ShowPage();
    }

    public string? ExecuteSelected()
    {
        if (SelectedIndex < 0 || SelectedIndex >= Results.Count) return null;
        var item = Results[SelectedIndex];
        return _executionService.Execute(item);
    }

    [RelayCommand] private void Hide() => IsVisible = false;

    [RelayCommand]
    private void OpenConfig()
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.yaml");
        Process.Start("notepad.exe", configPath);
    }

    [RelayCommand]
    private void ReloadConfig()
    {
        _configService.Reload();
        _pageSize = _configService.Current.UI.MaxVisibleItems;
        var cfg = _configService.Current;
        _ = Task.Run(async () =>
        {
            var indexService = ((App)System.Windows.Application.Current).Services.GetRequiredService<IFileIndexService>();
            await indexService.BuildCacheAsync(cfg.SearchPaths, cfg.FileTypes);
        });
        ConfigReloaded?.Invoke(cfg);
    }

    public event Action<ConfigData>? ConfigReloaded;

    private void ClearAll()
    {
        _allResults.Clear();
        Results.Clear();
        SelectedIndex = 0;
        TotalCount = 0;
        _page = 0;
    }

    public void ResetSearch()
    {
        SearchText = "";
        ClearAll();
        StatusText = "就绪";
    }
}

public static class AppServices
{
    public static IServiceProvider Services => ((App)System.Windows.Application.Current).Services;
}
