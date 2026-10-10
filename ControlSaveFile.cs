using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Hashing;
using System.Text;

namespace ControlSaveEditor;

public class MaterialEntry : INotifyPropertyChanged
{
    public ulong Gid { get; set; }
    public string Name { get; set; } = string.Empty;
    public int QuantityOffset { get; set; }

    private uint _quantity;
    public uint Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantity)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public class ModEntry : INotifyPropertyChanged
{
    public ulong Gid { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ParameterOffset { get; set; }
    public float OriginalParameter { get; set; }

    private float _parameter;
    public float Parameter
    {
        get => _parameter;
        set
        {
            if (_parameter != value)
            {
                _parameter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Parameter)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayStat)));
            }
        }
    }

    public string DisplayStat => Parameter >= 0 ? $"+{Parameter:0.#}%" : $"{Parameter:0.#}%";

    public event PropertyChangedEventHandler? PropertyChanged;
}

public class OutfitEntry : INotifyPropertyChanged
{
    public uint OutfitId { get; set; }
    private uint _status;
    public uint Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUnlocked)));
            }
        }
    }

    public bool IsUnlocked
    {
        get => Status > 0;
        set => Status = value ? (Status == 0 ? 1u : Status) : 0u;
    }

    public int StatusOffset { get; set; }
    public string Name => GetOutfitName(OutfitId);

    public event PropertyChangedEventHandler? PropertyChanged;

    public static string GetOutfitName(uint id) => id switch
    {
        2548037289 => "Civilian",
        2298332541 => "Janitor's Assistant",
        1617010995 => "Expedition Gear",
        2320418848 => "Asynchronous Suit",
        2272681173 => "Director's Suit",
        2756377209 => "Tactical Response (Pre-Order)",
        2961567163 => "Astral Dive Suit (Exclusive)",
        2294418169 => "Candidate P7",
        2872220490 => "Office Assistant",
        515893914 => "Golden Suit",
        3404528982 => "Urban Response (Exclusive)",
        2729692526 => "Extradimensional Suit (DLC)",
        _ => $"Outfit #{id}"
    };
}

public class ControlSaveFile
{
    public string FilePath { get; set; } = string.Empty;
    public byte[] RawBytes { get; set; } = Array.Empty<byte>();
    public uint StoredCrc32 { get; set; }
    public uint CalculatedCrc32 { get; set; }
    public bool IsCrcValid => StoredCrc32 == CalculatedCrc32;

    public ulong SourceCurrency { get; set; }
    public uint AbilityPoints { get; set; }

    public int PlayerPropsDataOffset { get; set; } = -1;
    public int SourceOffset { get; set; } = -1;
    public int AbilityPointsOffset { get; set; } = -1;

    public int SectorsVisitedCount { get; set; }
    public int FoundNarrativeObjectsCount { get; set; }
    public int UnlockedControlPointsCount { get; set; }

    public List<OutfitEntry> Outfits { get; } = new();
    public List<MaterialEntry> Materials { get; } = new();
    public List<ModEntry> Mods { get; } = new();

    public static string? AutoDetectSavesFolder()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string remedyDir = Path.Combine(localAppData, "Remedy", "Control");

        if (Directory.Exists(remedyDir))
        {
            foreach (var sub in Directory.GetDirectories(remedyDir))
            {
                if (Directory.GetDirectories(sub, "savegame-slot-*").Length > 0)
                {
                    return sub;
                }
            }
        }

        return null;
    }

    public static ControlSaveFile Load(string path)
    {
        var save = new ControlSaveFile { FilePath = path };
        save.RawBytes = File.ReadAllBytes(path);

        using var ms = new MemoryStream(save.RawBytes);
        using var reader = new BinaryReader(ms, Encoding.UTF8);

        // Header
        byte[] magic = reader.ReadBytes(8);
        save.StoredCrc32 = reader.ReadUInt32();

        // Recalculate CRC32 of buffer from offset 12 to EOF
        var crc = new Crc32();
        crc.Append(save.RawBytes.AsSpan(12));
        save.CalculatedCrc32 = BitConverter.ToUInt32(crc.GetCurrentHash(), 0);

        uint filenameLen = reader.ReadUInt32();
        string filename = Encoding.UTF8.GetString(reader.ReadBytes((int)filenameLen));
        byte scope = reader.ReadByte();
        uint unk3 = reader.ReadUInt32();
        uint numChunks = reader.ReadUInt32();

        for (int i = 0; i < numChunks; i++)
        {
            uint uidLow = reader.ReadUInt32();
            uint uidHigh = reader.ReadUInt32();
            uint size = reader.ReadUInt32();
            long dataStartPos = reader.BaseStream.Position;

            // PLAYER_PROPERTIES = 1721202367 (0x66986FBF)
            if (uidHigh == 1721202367)
            {
                save.PlayerPropsDataOffset = (int)dataStartPos;
                reader.BaseStream.Position = dataStartPos + 20; // Skip VersionData (20 bytes)

                save.SourceOffset = (int)reader.BaseStream.Position;
                save.SourceCurrency = reader.ReadUInt64();

                save.AbilityPointsOffset = (int)reader.BaseStream.Position;
                save.AbilityPoints = reader.ReadUInt32();

                uint numSectors = reader.ReadUInt32();
                save.SectorsVisitedCount = (int)numSectors;
                reader.BaseStream.Position += (numSectors * 8);

                uint numEnemies = reader.ReadUInt32();
                reader.BaseStream.Position += (numEnemies * 4);

                reader.BaseStream.Position += 8; // gidCurrentSector

                uint numNarrative = reader.ReadUInt32();
                save.FoundNarrativeObjectsCount = (int)numNarrative;
                reader.BaseStream.Position += (numNarrative * 8);

                uint numCP = reader.ReadUInt32();
                save.UnlockedControlPointsCount = (int)numCP;
            }
            // OUTFIT = 3357797087 (0xC8241ADF)
            else if (uidHigh == 3357797087)
            {
                reader.BaseStream.Position = dataStartPos + 20; // Skip VersionData
                uint currentOutfitId = reader.ReadUInt32();
                uint scriptOverrideOutfitId = reader.ReadUInt32();
                uint numOutfits = reader.ReadUInt32();

                for (int o = 0; o < numOutfits; o++)
                {
                    uint outfitObjVersion = reader.ReadUInt32();
                    uint outfitId = reader.ReadUInt32();
                    int statusOffset = (int)reader.BaseStream.Position;
                    uint status = reader.ReadUInt32();

                    save.Outfits.Add(new OutfitEntry
                    {
                        OutfitId = outfitId,
                        Status = status,
                        StatusOffset = statusOffset
                    });
                }
            }
            // INVENTORY = 3388499232 (0xC9F84D20)
            else if (uidHigh == 3388499232)
            {
                uint invObjVer = reader.ReadUInt32();
                uint numItems = reader.ReadUInt32();

                for (int j = 0; j < numItems; j++)
                {
                    uint itemObjVer = reader.ReadUInt32();
                    ulong gid = reader.ReadUInt64();
                    int paramOffset = (int)reader.BaseStream.Position;
                    float param = reader.ReadSingle();
                    uint overcharge = reader.ReadUInt32();
                    ulong persId = reader.ReadUInt64();
                    int qtyOffset = (int)reader.BaseStream.Position;
                    uint qty = reader.ReadUInt32();

                    string itemName = GameDatabase.GetItemName(gid);

                    if (GameDatabase.IsResource(gid, qty, param))
                    {
                        save.Materials.Add(new MaterialEntry
                        {
                            Gid = gid,
                            Name = itemName,
                            Quantity = qty,
                            QuantityOffset = qtyOffset
                        });
                    }
                    else if (GameDatabase.IsMod(gid, param))
                    {
                        save.Mods.Add(new ModEntry
                        {
                            Gid = gid,
                            Name = itemName,
                            Parameter = param,
                            OriginalParameter = param,
                            ParameterOffset = paramOffset
                        });
                    }
                }
            }

            reader.BaseStream.Position = dataStartPos + size;
        }

        return save;
    }

    public void UnlockAllOutfits()
    {
        foreach (var outfit in Outfits)
        {
            if (outfit.Status == 0)
            {
                outfit.Status = 1;
            }
        }
    }

    public void MaxAllMaterials(uint targetQty = 999)
    {
        foreach (var mat in Materials)
        {
            mat.Quantity = targetQty;
        }
    }

    public void BoostAllMods(float targetParam = 500f)
    {
        foreach (var mod in Mods)
        {
            // If mod was negative (e.g. -15% ammo cost or charge time), make it -90%
            if (mod.OriginalParameter < 0)
            {
                mod.Parameter = -90f;
            }
            else
            {
                mod.Parameter = targetParam;
            }
        }
    }

    public void ResetModsToVanilla()
    {
        foreach (var mod in Mods)
        {
            mod.Parameter = mod.OriginalParameter;
        }
    }

    public void Save(string? outputPath = null)
    {
        outputPath ??= FilePath;

        // Apply Source (Currency)
        if (SourceOffset >= 0)
        {
            byte[] srcBytes = BitConverter.GetBytes(SourceCurrency);
            Array.Copy(srcBytes, 0, RawBytes, SourceOffset, 8);
        }

        // Apply Ability Points
        if (AbilityPointsOffset >= 0)
        {
            byte[] apBytes = BitConverter.GetBytes(AbilityPoints);
            Array.Copy(apBytes, 0, RawBytes, AbilityPointsOffset, 4);
        }

        // Apply Outfits
        foreach (var outfit in Outfits)
        {
            if (outfit.StatusOffset > 0)
            {
                byte[] statusBytes = BitConverter.GetBytes(outfit.Status);
                Array.Copy(statusBytes, 0, RawBytes, outfit.StatusOffset, 4);
            }
        }

        // Apply Materials
        foreach (var mat in Materials)
        {
            if (mat.QuantityOffset > 0)
            {
                byte[] qtyBytes = BitConverter.GetBytes(mat.Quantity);
                Array.Copy(qtyBytes, 0, RawBytes, mat.QuantityOffset, 4);
            }
        }

        // Apply Mods
        foreach (var mod in Mods)
        {
            if (mod.ParameterOffset > 0)
            {
                byte[] paramBytes = BitConverter.GetBytes(mod.Parameter);
                Array.Copy(paramBytes, 0, RawBytes, mod.ParameterOffset, 4);
            }
        }

        // Recalculate CRC32
        var crc = new Crc32();
        crc.Append(RawBytes.AsSpan(12));
        byte[] crcBytes = crc.GetCurrentHash();
        Array.Copy(crcBytes, 0, RawBytes, 8, 4);

        // Auto backup
        if (File.Exists(outputPath))
        {
            string backupPath = outputPath + ".bak";
            if (!File.Exists(backupPath))
            {
                File.Copy(outputPath, backupPath, overwrite: false);
            }
        }

        File.WriteAllBytes(outputPath, RawBytes);
    }
}
