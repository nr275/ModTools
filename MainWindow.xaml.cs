using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Wpf.Ui.Controls;

namespace ControlSaveEditor;

public class SlotItem
{
    public string DisplayName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public override string ToString() => DisplayName;
}

public partial class MainWindow : FluentWindow
{
    private ControlSaveFile? _currentSave;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshSlots();
    }

    private void RefreshSlots()
    {
        CmbSlots.Items.Clear();
        string? root = ControlSaveFile.AutoDetectSavesFolder();

        if (!string.IsNullOrEmpty(root) && Directory.Exists(root))
        {
            var slotDirs = Directory.GetDirectories(root, "savegame-slot-*")
                                    .OrderBy(d => d)
                                    .ToList();

            foreach (var dir in slotDirs)
            {
                string chunkPath = Path.Combine(dir, "persistent.chunk");
                if (File.Exists(chunkPath))
                {
                    string dirName = Path.GetFileName(dir);
                    var item = new SlotItem
                    {
                        DisplayName = $"{dirName} ({new FileInfo(chunkPath).LastWriteTime:g})",
                        FilePath = chunkPath
                    };
                    CmbSlots.Items.Add(item);
                }
            }
        }

        if (CmbSlots.Items.Count > 0)
        {
            CmbSlots.SelectedIndex = 0;
        }
    }

    private void CmbSlots_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbSlots.SelectedItem is SlotItem item)
        {
            LoadFile(item.FilePath);
        }
    }

    private void BtnBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "Select Control persistent.chunk Save File",
            Filter = "Control Save Chunk (*.chunk;*_persistent)|*.chunk;*_persistent;*.*"
        };

        if (dlg.ShowDialog() == true)
        {
            LoadFile(dlg.FileName);
        }
    }

    private void BtnReload_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSave != null && File.Exists(_currentSave.FilePath))
        {
            LoadFile(_currentSave.FilePath);
        }
        else
        {
            RefreshSlots();
        }
    }

    private void LoadFile(string path)
    {
        try
        {
            _currentSave = ControlSaveFile.Load(path);

            PnlStatus.Visibility = Visibility.Visible;
            TxtStatusFile.Text = Path.GetFileName(Path.GetDirectoryName(path)) + " / " + Path.GetFileName(path);
            TxtStatusPath.Text = path;

            if (_currentSave.IsCrcValid)
            {
                BadgeCrc.Content = $"CRC32 Valid (0x{_currentSave.StoredCrc32:X8})";
                BadgeCrc.Appearance = ControlAppearance.Success;
            }
            else
            {
                BadgeCrc.Content = $"CRC Mismatch (Stored 0x{_currentSave.StoredCrc32:X8})";
                BadgeCrc.Appearance = ControlAppearance.Danger;
            }

            BadgeCollectibles.Content = $"{_currentSave.FoundNarrativeObjectsCount} Lore / {_currentSave.UnlockedControlPointsCount} CPs";

            TxtSource.Text = _currentSave.SourceCurrency.ToString();
            TxtAbilityPoints.Text = _currentSave.AbilityPoints.ToString();
            TxtExplorationSummary.Text = $"Unlocked {_currentSave.UnlockedControlPointsCount} fast-travel Control Points across {_currentSave.SectorsVisitedCount} sectors. Found {_currentSave.FoundNarrativeObjectsCount} collectibles & narrative objects.";

            LstMaterials.ItemsSource = null;
            LstMaterials.ItemsSource = _currentSave.Materials;

            LstMods.ItemsSource = null;
            LstMods.ItemsSource = _currentSave.Mods;

            LstOutfits.ItemsSource = null;
            LstOutfits.ItemsSource = _currentSave.Outfits;

            TxtSaveFeedback.Text = string.Empty;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to load save file:\n{ex.Message}", "Error Loading Save", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Currency & AP
    private void BtnAddSource100k_Click(object sender, RoutedEventArgs e)
    {
        if (ulong.TryParse(TxtSource.Text.Replace(",", "").Trim(), out ulong current))
        {
            TxtSource.Text = (current + 100_000).ToString();
        }
    }

    private void BtnAddSource1M_Click(object sender, RoutedEventArgs e)
    {
        if (ulong.TryParse(TxtSource.Text.Replace(",", "").Trim(), out ulong current))
        {
            TxtSource.Text = (current + 1_000_000).ToString();
        }
    }

    private void BtnSetSource10M_Click(object sender, RoutedEventArgs e)
    {
        TxtSource.Text = "10000000";
    }

    private void BtnAddAp10_Click(object sender, RoutedEventArgs e)
    {
        if (uint.TryParse(TxtAbilityPoints.Text.Trim(), out uint current))
        {
            TxtAbilityPoints.Text = (current + 10).ToString();
        }
    }

    private void BtnAddAp50_Click(object sender, RoutedEventArgs e)
    {
        if (uint.TryParse(TxtAbilityPoints.Text.Trim(), out uint current))
        {
            TxtAbilityPoints.Text = (current + 50).ToString();
        }
    }

    private void BtnMaxAp_Click(object sender, RoutedEventArgs e)
    {
        TxtAbilityPoints.Text = "100";
    }

    // Crafting Materials
    private void BtnMaxAllMaterials_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSave != null)
        {
            _currentSave.MaxAllMaterials(999);
            LstMaterials.ItemsSource = null;
            LstMaterials.ItemsSource = _currentSave.Materials;
            TxtSaveFeedback.Text = "All materials set to 999. Click 'Save & Apply' to commit.";
        }
    }

    // Mods
    private void BtnResetMods_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSave != null)
        {
            _currentSave.ResetModsToVanilla();
            LstMods.ItemsSource = null;
            LstMods.ItemsSource = _currentSave.Mods;
            TxtSaveFeedback.Text = "Mods reset to original stats.";
        }
    }

    private void BtnBoostMods500_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSave != null)
        {
            _currentSave.BoostAllMods(500f);
            LstMods.ItemsSource = null;
            LstMods.ItemsSource = _currentSave.Mods;
            TxtSaveFeedback.Text = "All mods boosted to +500%! Click 'Save & Apply' to commit.";
        }
    }

    private void BtnBoostMods1000_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSave != null)
        {
            _currentSave.BoostAllMods(1000f);
            LstMods.ItemsSource = null;
            LstMods.ItemsSource = _currentSave.Mods;
            TxtSaveFeedback.Text = "⚡ GOD MODE: All mods boosted to +1000%! Click 'Save & Apply' to commit.";
        }
    }

    // Outfits
    private void BtnUnlockAllOutfits_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSave != null)
        {
            _currentSave.UnlockAllOutfits();
            LstOutfits.ItemsSource = null;
            LstOutfits.ItemsSource = _currentSave.Outfits;
            TxtSaveFeedback.Text = "All 12 outfits unlocked!";
        }
    }

    // Save All
    private void BtnSaveChanges_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSave == null)
        {
            System.Windows.MessageBox.Show("No save file loaded.", "Notice", System.Windows.MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!ulong.TryParse(TxtSource.Text.Replace(",", "").Trim(), out ulong source))
        {
            System.Windows.MessageBox.Show("Invalid Source (currency) number.", "Validation Error", System.Windows.MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!uint.TryParse(TxtAbilityPoints.Text.Trim(), out uint ap))
        {
            System.Windows.MessageBox.Show("Invalid Ability Points number.", "Validation Error", System.Windows.MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            _currentSave.SourceCurrency = source;
            _currentSave.AbilityPoints = ap;

            _currentSave.Save();

            // Reload to re-verify CRC32 and show success
            LoadFile(_currentSave.FilePath);
            TxtSaveFeedback.Text = "✓ Saved successfully! Automatic backup saved to .bak";
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to save changes:\n{ex.Message}", "Save Error", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}