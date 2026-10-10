using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ControlSaveEditor;

public static class GameDatabase
{
    private static readonly Dictionary<string, string> _items = new(StringComparer.OrdinalIgnoreCase);

    static GameDatabase()
    {
        try
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream("ControlSaveEditor.GameItems.json");
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();
                var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (parsed != null)
                {
                    foreach (var kvp in parsed)
                    {
                        _items[kvp.Key] = kvp.Value;
                    }
                }
            }
        }
        catch
        {
            // Fallback gracefully
        }
    }

    public static string GetItemName(ulong gid)
    {
        string hex = gid.ToString("x");
        if (_items.TryGetValue(hex, out string? name))
        {
            return CleanName(name);
        }

        // Try padded 16-hex format
        string hex16 = gid.ToString("x16");
        if (_items.TryGetValue(hex16, out string? name16))
        {
            return CleanName(name16);
        }

        return $"Item [0x{hex}]";
    }

    public static bool IsResource(ulong gid, uint quantity, float parameter)
    {
        string hex = gid.ToString("x");
        if (_items.TryGetValue(hex, out string? name))
        {
            if (name.StartsWith("Resource", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Contains("Token", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.StartsWith("Weapon Player", StringComparison.OrdinalIgnoreCase)) return false;
        }

        return parameter == 0 && quantity > 1;
    }

    public static bool IsMod(ulong gid, float parameter)
    {
        string hex = gid.ToString("x");
        if (_items.TryGetValue(hex, out string? name))
        {
            if (name.Contains("Mod", StringComparison.OrdinalIgnoreCase)) return true;
        }

        return parameter != 0;
    }

    private static string CleanName(string raw)
    {
        if (raw.StartsWith("Resource: ", StringComparison.OrdinalIgnoreCase))
        {
            return raw.Substring("Resource: ".Length);
        }
        return raw;
    }
}
