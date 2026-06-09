using Quera.Helpers;

namespace Quera.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private readonly DispatcherTimer _deactivateTimer;
    private bool _needsCenter;

    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;

        _deactivateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _deactivateTimer.Tick += OnDeactivateTimerTick;

        SourceInitialized += OnSourceInitialized;
        Loaded += OnWindowLoaded;
        SizeChanged += OnWindowSizeChanged;
        KeyDown += OnWindowKeyDown;
        _vm.PropertyChanged += OnVmPropertyChanged;
        _vm.Results.CollectionChanged += OnResultsChanged;

        var cfg = AppServices.Services.GetRequiredService<IConfigService>();
        Width = cfg.Current.Width;
        Opacity = cfg.Current.Opacity;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        AcrylicHelper.ApplyBackdrop(this);
    }

    private void OnOuterBorderSizeChanged(object sender, SizeChangedEventArgs e)
    {
        OuterBorder.Clip = new RectangleGeometry(
            new Rect(0, 0, OuterBorder.ActualWidth, OuterBorder.ActualHeight), 20, 20);
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        // Handles case when size hasn't changed (e.g., first toggle after hide)
        DeferredCenter();
    }

    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Handles case when content changes size (e.g., results appear/disappear)
        DeferredCenter();
    }

    private void DeferredCenter()
    {
        if (!_needsCenter) return;
        Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
        {
            if (_needsCenter)
            {
                _needsCenter = false;
                var workArea = System.Windows.SystemParameters.WorkArea;
                Left = workArea.Left + (workArea.Width - ActualWidth) / 2;
                Top = workArea.Top + (workArea.Height - ActualHeight) / 2;
            }
        });
    }

    private void OnResultsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // WPF auto-sizes via SizeToContent=Height, but we need to re-center
        _needsCenter = true;
    }

    private void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsVisible))
        {
            if (_vm.IsVisible)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    _needsCenter = true;
                    Show();
                    Activate();
                });
            }
            else
            {
                Dispatcher.BeginInvoke(() => Hide());
            }
        }
        else if (e.PropertyName == nameof(MainViewModel.HasSearchText))
        {
            _needsCenter = true;
        }
    }

    public void ToggleVisibility()
    {
        if (IsVisible)
        {
            Hide();
            _vm.IsVisible = false;
        }
        else
        {
            _vm.ResetSearch();
            _needsCenter = true;
            Show();
            Activate();
            _vm.IsVisible = true;
            SearchTextBox.Focus();
        }
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);
        _deactivateTimer.Start();
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        _deactivateTimer.Stop();
    }

    private void OnDeactivateTimerTick(object? sender, EventArgs e)
    {
        _deactivateTimer.Stop();
        if (!IsActive)
        {
            Hide();
            _vm.IsVisible = false;
        }
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            e.Handled = true;
            _vm.SelectNextCommand.Execute(null);
        }
        else if (e.Key == Key.Up)
        {
            e.Handled = true;
            _vm.SelectPreviousCommand.Execute(null);
        }
        else if (e.Key == Key.Enter)
        {
            e.Handled = true;
            ExecuteSelectedAndHide();
        }
        else if (e.Key == Key.Tab && _vm.SelectedIndex >= 0 && _vm.SelectedIndex < _vm.Results.Count)
        {
            if (_vm.Results[_vm.SelectedIndex].Type == SearchResultType.SearchHint)
            {
                e.Handled = true;
                ExecuteSelectedAndHide();
            }
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Hide();
            _vm.IsVisible = false;
        }
        else if (e.Key == Key.OemComma && Keyboard.Modifiers == ModifierKeys.Control)
        {
            e.Handled = true;
            _vm.OpenConfigCommand.Execute(null);
        }
        else if (e.Key == Key.R && Keyboard.Modifiers == ModifierKeys.Control)
        {
            e.Handled = true;
            _vm.ReloadConfigCommand.Execute(null);
        }
    }

    private void ExecuteSelectedAndHide()
    {
        var completion = _vm.ExecuteSelected();
        if (!string.IsNullOrEmpty(completion))
        {
            _vm.SearchText = completion;
            SearchTextBox.Focus();
            SearchTextBox.CaretIndex = SearchTextBox.Text.Length;
        }
        else
        {
            Hide();
            _vm.IsVisible = false;
        }
    }
}
