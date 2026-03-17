using Microsoft.UI.Xaml;

namespace WangyiMCCheckworld;

public partial class App : Application
{
    public App()
    {
        // Initialize resources programmatically instead of using XAML
        var resources = new Microsoft.UI.Xaml.ResourceDictionary();

        // Add XamlControlsResources
        var xamlControlsResources = new Microsoft.UI.Xaml.Controls.XamlControlsResources();
        resources.MergedDictionaries.Add(xamlControlsResources.MergedDictionaries[0]);

        // Add custom colors
        resources.Add("ApplicationBackgroundBrush", new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White));
        resources.Add("TextFillColorPrimaryBrush", new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black));
        resources.Add("TextFillColorSecondaryBrush", new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray));
        resources.Add("ControlFillColorDefaultBrush", new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGray));
        resources.Add("ControlStrongFillColorDefaultBrush", new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGray));
        resources.Add("ControlStrokeColorDefaultBrush", new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkGray));
        resources.Add("AccentFillColorDefaultBrush", new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.CornflowerBlue));

        this.Resources = resources;
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();

        // Set window size after creation
        if (m_window is MainWindow mainWindow)
        {
            // Try to set size using AppWindow if available
            try
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                appWindow?.Resize(new Windows.Graphics.SizeInt32 { Width = 1100, Height = 700 });
            }
            catch
            {
                // Ignore if AppWindow is not available
            }
        }

        m_window.Activate();
    }

    private Microsoft.UI.Xaml.Window? m_window;
}