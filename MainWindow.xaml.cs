using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.IO.Compression;

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
    private string _worldsPath = string.Empty;

    public MainWindow()
    {
        this.InitializeComponent();
        WorldListView.ItemsSource = _worlds;
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

            StatusText.Text = $"共 {_worlds.Count} 个存档";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"加载失败: {ex.Message}";
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

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        _worlds.Clear();
        await LoadWorldsAsync();
    }

    private async void BackupButton_Click(object sender, RoutedEventArgs e)
    {
        if (WorldListView.SelectedItem is WorldInfo selected)
        {
            try
            {
                StatusText.Text = "正在备份...";
                var zipPath = Path.Combine(_worldsPath, $"{selected.FolderName}.zip");
                await Task.Run(() => ZipFile.CreateFromDirectory(selected.FullPath, zipPath));
                StatusText.Text = $"备份完成: {Path.GetFileName(zipPath)}";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"备份失败: {ex.Message}";
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