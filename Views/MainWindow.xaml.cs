using Quera.Helpers;

namespace Quera.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private readonly DispatcherTimer _deactivateTimer;
    private bool _needsCenter;
    private double _anchorTop;
    private readonly IConfigService _cfg;

    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        _cfg = AppServices.Services.GetRequiredService<IConfigService>();
        DataContext = vm;

        _deactivateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(_cfg.Current.HideDelayMs)
        };
        _deactivateTimer.Tick += OnDeactivateTimerTick;

        SourceInitialized += OnSourceInitialized;
        SizeChanged += OnSizeChanged;
        PreviewKeyDown += OnWindowPreviewKeyDown;

        Width = _cfg.Current.Width;
        Opacity = _cfg.Current.Opacity;
        Topmost = _cfg.Current.Window.AlwaysOnTop;
        ApplyUIConfig(_cfg.Current);
        _vm.ConfigReloaded += c => Dispatcher.Invoke(() => ApplyUIConfig(c));
    }

    private void ApplyUIConfig(ConfigData c)
    {
        var ui = c.UI;
        var l = c.Layout;

        Chrome.CornerRadius = new CornerRadius(ui.BorderRadius);
        OuterBorder.CornerRadius = new CornerRadius(ui.BorderRadius);

        OuterGrid.Margin = new Thickness(l.OuterMargin);
        SearchCard.Padding = new Thickness(l.SearchPadding);
        SearchCard.CornerRadius = new CornerRadius(ui.BorderRadius * 0.7);
        ResultsCard.CornerRadius = new CornerRadius(ui.BorderRadius * 0.7);
        ResultsCard.Padding = new Thickness(l.ResultsPadding);
        ResultsCard.Margin = new Thickness(0, l.CardGap, 0, 0);

        SearchTextBox.FontFamily = new System.Windows.Media.FontFamily(ui.FontFamily);
        SearchTextBox.FontSize = ui.FontSizeSearch;
        SearchIcon.FontSize = ui.FontSizeSearch;

        var sb = c.SearchBox;
        SearchIcon.Text = sb.Icon;
        EscText.Text = sb.EscHint;

        StatusBar.Visibility = ui.ShowStatusBar ? Visibility.Visible : Visibility.Collapsed;
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
        if (IsVisible) { FadeOut(); return; }
        _vm.ResetSearch();
        _needsCenter = true;
        Opacity = 0;
        Show();
        Activate();
        _vm.IsVisible = true;
        SearchTextBox.Focus();
        FadeIn();
    }

    private async void FadeIn()
    {
        for (double o = 0; o <= _cfg.Current.Opacity; o += 0.08)
        {
            Opacity = o;
            await Task.Delay(12);
        }
        Opacity = _cfg.Current.Opacity;
    }

    private async void FadeOut()
    {
        var target = Opacity;
        for (double o = target; o >= 0; o -= 0.1)
        {
            Opacity = o;
            await Task.Delay(8);
        }
        Opacity = 0;
        Hide();
        _vm.IsVisible = false;
        Opacity = _cfg.Current.Opacity;
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
            case Key.Tab when _vm.SelectedIndex >= 0 && _vm.SelectedIndex < _vm.Results.Count
                && _vm.Results[_vm.SelectedIndex].Type == SearchResultType.SearchHint:
                e.Handled = true; ExecAndHide(); break;
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
