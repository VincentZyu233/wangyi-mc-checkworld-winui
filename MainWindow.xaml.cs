using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace WangyiMCCheckworld;

public class MainWindow : Window
{
    private readonly ObservableCollection<WorldInfo> _worlds = new();
    private readonly TextBlock _statusText;
    private SortColumn _sortColumn = SortColumn.LastModified;
    private bool _sortAscending = false;

    public MainWindow()
    {
        this.Title = "网易MC检查世界";

        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = CreateHeader();
        root.Children.Add(header);
        Grid.SetRow(header, 0);

        var listView = CreateListView();
        root.Children.Add(listView);
        Grid.SetRow(listView, 1);

        _statusText = new TextBlock
        {
            Text = "正在加载存档列表...",
            Margin = new Thickness(10)
        };
        root.Children.Add(_statusText);
        Grid.SetRow(_statusText, 2);

        this.Content = root;

        _ = LoadWorldsAsync();
    }

    private FrameworkElement CreateHeader()
    {
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
            Padding = new Thickness(8)
        };

        headerPanel.Children.Add(CreateHeaderButton("世界名", SortColumn.Name));
        headerPanel.Children.Add(CreateHeaderButton("文件夹", SortColumn.Folder));
        headerPanel.Children.Add(CreateHeaderButton("大小", SortColumn.Size));
        headerPanel.Children.Add(CreateHeaderButton("修改时间", SortColumn.LastModified));

        return headerPanel;
    }

    private Button CreateHeaderButton(string text, SortColumn column)
    {
        var btn = new Button
        {
            Content = text,
            Margin = new Thickness(4, 0, 4, 0)
        };

        btn.Click += (_, _) =>
        {
            if (_sortColumn == column)
                _sortAscending = !_sortAscending;
            else
            {
                _sortColumn = column;
                _sortAscending = true;
            }

            ApplySort();
        };

        return btn;
    }

    private ListView CreateListView()
    {
        var listView = new ListView { ItemsSource = _worlds, Margin = new Thickness(8) };

        // Simplified: no custom ItemTemplate, will display ToString or default
        return listView;
    }

    private async Task LoadWorldsAsync()
    {
        var worldsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MinecraftPC_Netease_PB", "minecraftWorlds");
        if (!Directory.Exists(worldsDir))
        {
            _statusText.Text = "找不到存档目录：" + worldsDir;
            return;
        }

        var dirs = Directory.EnumerateDirectories(worldsDir);
        var list = await Task.Run(() => dirs
            .Select(d => CreateWorldInfo(new DirectoryInfo(d)))
            .ToList());

        _worlds.Clear();
        foreach (var item in list) _worlds.Add(item);

        ApplySort();
        _statusText.Text = $"共 {_worlds.Count} 个存档";
    }

    private static WorldInfo CreateWorldInfo(DirectoryInfo dir)
    {
        var info = new WorldInfo
        {
            Name = dir.Name,
            Folder = dir.FullName,
            LastModified = dir.LastWriteTime,
            SizeBytes = GetDirectorySize(dir)
        };
        return info;
    }

    private static long GetDirectorySize(DirectoryInfo dir)
    {
        long size = 0;
        try
        {
            foreach (var file in dir.EnumerateFiles("*", SearchOption.AllDirectories))
                size += file.Length;
        }
        catch
        {
            // ignore access errors
        }

        return size;
    }

    private void ApplySort()
    {
        var sorted = _sortColumn switch
        {
            SortColumn.Name => _sortAscending ? _worlds.OrderBy(w => w.Name) : _worlds.OrderByDescending(w => w.Name),
            SortColumn.Folder => _sortAscending ? _worlds.OrderBy(w => w.Folder) : _worlds.OrderByDescending(w => w.Folder),
            SortColumn.Size => _sortAscending ? _worlds.OrderBy(w => w.SizeBytes) : _worlds.OrderByDescending(w => w.SizeBytes),
            SortColumn.LastModified => _sortAscending ? _worlds.OrderBy(w => w.LastModified) : _worlds.OrderByDescending(w => w.LastModified),
            _ => _worlds.OrderBy(w => w.LastModified)
        };

        var sortedList = sorted.ToList();
        _worlds.Clear();
        foreach (var w in sortedList) _worlds.Add(w);
    }
}

public enum SortColumn
{
    Name,
    Folder,
    Size,
    LastModified
}

public class WorldInfo
{
    public string Name { get; set; } = string.Empty;
    public string Folder { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime LastModified { get; set; }

    public string SizeText => SizeBytes >= 0 ? FormatBytes(SizeBytes) : "-";
    public string LastModifiedText => LastModified == default ? "-" : LastModified.ToString("yyyy-MM-dd HH:mm:ss");

    public override string ToString()
    {
        return $"{Name}\t{Folder}\t{SizeText}\t{LastModifiedText}";
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return bytes + " B";
        var kb = bytes / 1024.0;
        if (kb < 1024) return kb.ToString("0.0") + " KB";
        var mb = kb / 1024.0;
        if (mb < 1024) return mb.ToString("0.0") + " MB";
        var gb = mb / 1024.0;
        return gb.ToString("0.0") + " GB";
    }
}