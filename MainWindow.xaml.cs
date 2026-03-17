using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WangyiMCCheckworld;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        this.Title = "网易MC检查世界";
        this.Width = 1100;
        this.Height = 700;

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