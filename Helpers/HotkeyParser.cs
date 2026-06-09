namespace Quera.Helpers;

internal static class HotkeyParser
{
    private static readonly Dictionary<string, uint> SpecialKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["space"] = NativeMethods.VK_SPACE,
        ["enter"] = NativeMethods.VK_RETURN,
        ["tab"] = NativeMethods.VK_TAB,
        ["esc"] = NativeMethods.VK_ESCAPE,
        ["escape"] = NativeMethods.VK_ESCAPE,
        ["f1"] = NativeMethods.VK_F1, ["f2"] = NativeMethods.VK_F2,
        ["f3"] = NativeMethods.VK_F3, ["f4"] = NativeMethods.VK_F4,
        ["f5"] = NativeMethods.VK_F5, ["f6"] = NativeMethods.VK_F6,
        ["f7"] = NativeMethods.VK_F7, ["f8"] = NativeMethods.VK_F8,
        ["f9"] = NativeMethods.VK_F9, ["f10"] = NativeMethods.VK_F10,
        ["f11"] = NativeMethods.VK_F11, ["f12"] = NativeMethods.VK_F12,
    };

    public static (uint mod, uint vk) Parse(string hotkeyStr)
    {
        uint mod = 0;
        uint vk = 0;

        var lower = (hotkeyStr ?? "alt+space").ToLowerInvariant();

        if (lower.Contains("ctrl") || lower.Contains("control"))
            mod |= NativeMethods.MOD_CONTROL;
        if (lower.Contains("alt"))
            mod |= NativeMethods.MOD_ALT;
        if (lower.Contains("shift"))
            mod |= NativeMethods.MOD_SHIFT;
        if (lower.Contains("win"))
            mod |= NativeMethods.MOD_WIN;

        var parts = lower.Split('+');
        if (parts.Length > 0)
        {
            var keyStr = parts[^1].Trim().ToLowerInvariant();

            if (SpecialKeys.TryGetValue(keyStr, out var specialVk))
            {
                vk = specialVk;
            }
            else if (keyStr.Length == 1)
            {
                vk = (uint)char.ToUpperInvariant(keyStr[0]);
            }
        }

        return (mod, vk);
    }
}
