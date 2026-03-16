using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace WangyiMCCheckworld;

public class WorldInfo
{
    public string FolderName { get; set; } = string.Empty;
    public string WorldName { get; set; } = string.Empty;
    public string LastModified { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
}

public partial class MainWindow : Window
{
    private ObservableCollection<WorldInfo> _worlds = new();
    private ObservableCollection<WorldInfo> _filteredWorlds = new();
    private string _worldsPath = string.Empty;

    public MainWindow()
    {
        this.InitializeComponent();
        WorldDataGrid.ItemsSource = _filteredWorlds;
        LoadWorldsPath();
        _ = LoadWorldsAsync();
    }

    private void LoadWorldsPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var neteasePath = Path.Combine(appData, "MinecraftPC_Netease_PB", "minecraftWorlds");
        
        if (Directory.Exists(neteasePath))
        {
            _worldsPath = neteasePath;
        }
        else
        {
            StatusText.Text = "未找到网易MC存档目录";
        }
    }

    private async Task LoadWorldsAsync()
    {
        if (string.IsNullOrEmpty(_worldsPath)) return;

        LoadingRing.Visibility = Visibility.Visible;
        LoadingRing.IsActive = true;
        StatusText.Text = "正在加载存档...";

        try
        {
            await Task.Run(() =>
            {
                var dirs = Directory.GetDirectories(_worldsPath);
                
                foreach (var dir in dirs)
                {
                    var dirName = Path.GetFileName(dir);
                    if (dirName.StartsWith("+++")) continue;

                    var levelnameFile = Path.Combine(dir, "levelname.txt");
                    var worldName = "未知";
                    if (File.Exists(levelnameFile))
                    {
                        try { worldName = File.ReadAllText(levelnameFile).Trim(); }
                        catch { }
                    }

                    var lastModified = "未知";
                    var levelDat = Path.Combine(dir, "level.dat");
                    if (File.Exists(levelDat))
                    {
                        var info = new FileInfo(levelDat);
                        lastModified = info.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
                    }

                    var size = "未知";
                    try
                    {
                        var sizeBytes = GetDirectorySize(dir);
                        size = FormatSize(sizeBytes);
                    }
                    catch { }

                    var info2 = new WorldInfo
                    {
                        FolderName = dirName,
                        WorldName = worldName,
                        LastModified = lastModified,
                        Size = size,
                        FullPath = dir
                    };

                    DispatcherQueue.TryEnqueue(() => _worlds.Add(info2));
                }
            });

            _filteredWorlds = new ObservableCollection<WorldInfo>(_worlds);
            WorldDataGrid.ItemsSource = _filteredWorlds;
            StatusText.Text = $"共 {_worlds.Count} 个存档";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"加载失败: {ex.Message}";
        }
        finally
        {
            LoadingRing.Visibility = Visibility.Collapsed;
            LoadingRing.IsActive = false;
        }
    }

    private long GetDirectorySize(string path)
    {
        long size = 0;
        try
        {
            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                try { size += new FileInfo(file).Length; }
                catch { }
            }
        }
        catch { }
        return size;
    }

    private string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = SearchBox.Text.ToLower();
        if (string.IsNullOrEmpty(searchText))
        {
            _filteredWorlds = new ObservableCollection<WorldInfo>(_worlds);
        }
        else
        {
            _filteredWorlds = new ObservableCollection<WorldInfo>(
                _worlds.Where(w => 
                    w.FolderName.ToLower().Contains(searchText) || 
                    w.WorldName.ToLower().Contains(searchText)));
        }
        WorldDataGrid.ItemsSource = _filteredWorlds;
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        _worlds.Clear();
        await LoadWorldsAsync();
    }

    private async void BackupButton_Click(object sender, RoutedEventArgs e)
    {
        if (WorldDataGrid.SelectedItem is WorldInfo selected)
        {
            try
            {
                StatusText.Text = "正在备份...";
                var zipPath = Path.Combine(_worldsPath, $"{selected.FolderName}.zip");
                await Task.Run(() => System.IO.Compression.ZipFile.CreateFromDirectory(selected.FullPath, zipPath));
                StatusText.Text = $"备份完成: {Path.GetFileName(zipPath)}";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"备份失败: {ex.Message}";
            }
        }
    }

    private void RenameButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is WorldInfo info)
        {
            var dialog = new ContentDialog
            {
                Title = "重命名存档",
                Content = new TextBox { Text = info.WorldName, PlaceholderText = "输入新的世界名称" },
                PrimaryButtonText = "确定",
                CloseButtonText = "取消"
            };
            dialog.PrimaryButtonClick += async (s, args) =>
            {
                var textBox = (TextBox)dialog.Content;
                var newName = textBox.Text.Trim();
                if (!string.IsNullOrEmpty(newName))
                {
                    var levelnamePath = Path.Combine(info.FullPath, "levelname.txt");
                    await File.WriteAllTextAsync(levelnamePath, newName);
                    info.WorldName = newName;
                    await LoadWorldsAsync();
                    StatusText.Text = "重命名成功";
                }
            };
            _ = dialog.ShowAsync();
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is WorldInfo info)
        {
            var dialog = new ContentDialog
            {
                Title = "删除确认",
                Content = $"确定要删除存档 \"{info.WorldName}\" 吗？此操作不可恢复！",
                PrimaryButtonText = "删除",
                CloseButtonText = "取消",
                PrimaryButtonStyle = new Style { TargetType = typeof(Button) }
            };
            
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    Directory.Delete(info.FullPath, true);
                    _worlds.Remove(info);
                    _filteredWorlds.Remove(info);
                    StatusText.Text = "删除成功";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"删除失败: {ex.Message}";
                }
            }
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.Minimize();
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.WindowState == WindowState.Maximized)
            this.WindowState = WindowState.Normal;
        else
            this.WindowState = WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}