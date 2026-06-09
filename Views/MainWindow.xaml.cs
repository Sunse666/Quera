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

        var cfg = AppServices.Services.GetRequiredService<IConfigService>();
        _deactivateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(cfg.Current.HideDelayMs)
        };
        _deactivateTimer.Tick += OnDeactivateTimerTick;

        SourceInitialized += OnSourceInitialized;
        SizeChanged += (_, _) => { if (_needsCenter) CenterOnScreen(); };
        PreviewKeyDown += OnWindowPreviewKeyDown;

        Width = cfg.Current.Width;
        Opacity = cfg.Current.Opacity;
        ResultsListView.MaxHeight = cfg.Current.UI.MaxVisibleItems * cfg.Current.UI.ItemHeight;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        AcrylicHelper.ApplyBackdrop(this);
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

    private void CenterOnScreen()
    {
        _needsCenter = false;
        var wa = System.Windows.SystemParameters.WorkArea;
        Left = wa.Left + (wa.Width - ActualWidth) / 2;
        Top = wa.Top + (wa.Height - ActualHeight) / 2;
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
            case Key.Tab when _vm.SelectedIndex >= 0 && _vm.SelectedIndex < _vm.Results.Count
                && _vm.Results[_vm.SelectedIndex].Type == SearchResultType.SearchHint:
                e.Handled = true; ExecAndHide(); break;
            case Key.OemComma when Keyboard.Modifiers == ModifierKeys.Control:
                e.Handled = true; _vm.OpenConfigCommand.Execute(null); break;
            case Key.R when Keyboard.Modifiers == ModifierKeys.Control:
                e.Handled = true; _vm.ReloadConfigCommand.Execute(null); break;
        }
    }

    private void ExecAndHide()
    {
        var s = _vm.ExecuteSelected();
        if (!string.IsNullOrEmpty(s))
        {
            _vm.SearchText = s;
            SearchTextBox.Focus();
            SearchTextBox.CaretIndex = SearchTextBox.Text.Length;
        }
        else { Hide(); _vm.IsVisible = false; }
    }
}
