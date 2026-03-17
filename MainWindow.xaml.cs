using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI;

namespace WangyiMCCheckworld;

public class MainWindow : Window
{
    public MainWindow()
    {
        this.Title = "网易MC检查世界";

        // Set window size using AppWindow
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
        appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 1100, Height = 700 });

        // Create the UI programmatically
        var grid = new Grid();
        var textBlock = new TextBlock
        {
            Text = "Hello World",
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
        };

        grid.Children.Add(textBlock);
        this.Content = grid;
    }
}