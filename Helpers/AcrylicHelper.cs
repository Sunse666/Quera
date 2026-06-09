namespace Quera.Helpers;

internal static class AcrylicHelper
{
    public static void ApplyBackdrop(Window window)
    {
        var hwnd = new WindowInteropHelper(window).EnsureHandle();
        if (Environment.OSVersion.Version.Build >= 22621)
        {
            int micaAlways = 1;
            NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_MICA_ALWAYS, ref micaAlways, sizeof(int));

            int darkMode = 1;
            NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

            int cornerPreference = 2;
            NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));
        }
    }
}
