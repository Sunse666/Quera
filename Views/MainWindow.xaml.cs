using Quera.Helpers;

namespace Quera.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private readonly DispatcherTimer _deactivateTimer;
    private bool _needsCenter;
    private double _anchorTop;

    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        var cfg = AppServices.Services.GetRequiredService<IConfigService>();
        DataContext = vm;

        _deactivateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(cfg.Current.HideDelayMs)
        };
        _deactivateTimer.Tick += OnDeactivateTimerTick;

        SourceInitialized += OnSourceInitialized;
        SizeChanged += OnSizeChanged;
        PreviewKeyDown += OnWindowPreviewKeyDown;

        Width = cfg.Current.Width;
        Opacity = cfg.Current.Opacity;
    }

    private void OnSourceInitialized(object? sender, EventArgs e) => AcrylicHelper.ApplyBackdrop(this);

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_needsCenter)
        {
            _needsCenter = false;
            _anchorTop = Top;
            var wa = System.Windows.SystemParameters.WorkArea;
            Left = wa.Left + (wa.Width - ActualWidth) / 2;
        }
        Top = _anchorTop;
    }

    public void ToggleVisibility()
    {
        if (IsVisible) { Hide(); _vm.IsVisible = false; return; }
        _vm.ResetSearch();
        _needsCenter = true;
        Show();
        Activate();
        _vm.IsVisible = true;
        SearchTextBox.Focus();
    }

    protected override void OnDeactivated(EventArgs e) { base.OnDeactivated(e); _deactivateTimer.Start(); }
    protected override void OnActivated(EventArgs e) { base.OnActivated(e); _deactivateTimer.Stop(); }

    private void OnDeactivateTimerTick(object? sender, EventArgs e)
    {
        _deactivateTimer.Stop();
        if (!IsActive) { Hide(); _vm.IsVisible = false; }
    }

    private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down: e.Handled = true; _vm.SelectNextCommand.Execute(null); break;
            case Key.Up: e.Handled = true; _vm.SelectPreviousCommand.Execute(null); break;
            case Key.Enter: e.Handled = true; ExecAndHide(); break;
            case Key.Escape: e.Handled = true; Hide(); _vm.IsVisible = false; break;
            case Key.Tab when Keyboard.Modifiers == ModifierKeys.Shift:
                e.Handled = true; _vm.PrevPage(); break;
            case Key.Tab:
                e.Handled = true; _vm.NextPage(); break;
            case Key.OemComma when Keyboard.Modifiers == ModifierKeys.Control:
                e.Handled = true; _vm.OpenConfigCommand.Execute(null); break;
            case Key.R when Keyboard.Modifiers == ModifierKeys.Control:
                e.Handled = true; _vm.ReloadConfigCommand.Execute(null); break;
        }
    }

    private void ExecAndHide()
    {
        var s = _vm.ExecuteSelected();
        if (!string.IsNullOrEmpty(s)) { _vm.SearchText = s; SearchTextBox.Focus(); SearchTextBox.CaretIndex = SearchTextBox.Text.Length; }
        else { Hide(); _vm.IsVisible = false; }
    }
}
