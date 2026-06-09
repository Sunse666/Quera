namespace Quera.Services;

public interface IHotkeyService
{
    event Action? HotkeyPressed;
    void Initialize(Window window);
    bool Register(string hotkeyStr);
    void Unregister();
}
