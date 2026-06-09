using Quera.Helpers;

namespace Quera.Services;

public class HotkeyService : IHotkeyService, IDisposable
{
    private readonly ILogger<HotkeyService> _logger;
    private HwndSource? _hwndSource;
    private int _hotkeyId = 9000;
    private (uint mod, uint vk) _currentHotkey;
    private bool _registered;

    public event Action? HotkeyPressed;

    public HotkeyService(ILogger<HotkeyService> logger)
    {
        _logger = logger;
    }

    public void Initialize(Window window)
    {
        _hwndSource = PresentationSource.FromVisual(window) as HwndSource;
        _hwndSource?.AddHook(WndProc);
    }

    public bool Register(string hotkeyStr)
    {
        if (_hwndSource == null) return false;

        Unregister();

        _currentHotkey = HotkeyParser.Parse(hotkeyStr);

        if (_currentHotkey.vk == 0)
        {
            _logger.LogWarning("Failed to parse hotkey: {Hotkey}", hotkeyStr);
            return false;
        }

        _registered = NativeMethods.RegisterHotKey(
            _hwndSource.Handle,
            _hotkeyId,
            _currentHotkey.mod,
            _currentHotkey.vk);

        if (!_registered)
            _logger.LogWarning("Failed to register hotkey: {Hotkey}", hotkeyStr);

        return _registered;
    }

    public void Unregister()
    {
        if (_registered && _hwndSource != null)
        {
            NativeMethods.UnregisterHotKey(_hwndSource.Handle, _hotkeyId);
            _registered = false;
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == _hotkeyId)
        {
            HotkeyPressed?.Invoke();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        Unregister();
        _hwndSource?.RemoveHook(WndProc);
        _hwndSource?.Dispose();
    }
}
