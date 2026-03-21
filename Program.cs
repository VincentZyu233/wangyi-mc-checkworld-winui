using Microsoft.UI.Xaml;
using System;

namespace WangyiMCCheckworld;

public static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();

            Microsoft.UI.Xaml.Application.Start((p) => {
                try
                {
                    var context = new Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(
                        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
                    SynchronizationContext.SetSynchronizationContext(context);
                    new App();
                }
                catch (Exception ex)
                {
                    ShowError("Application startup failed", ex.ToString());
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            ShowError("Application initialization failed", ex.ToString());
            throw;
        }
    }

    private static void ShowError(string title, string message)
    {
        try
        {
            var logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WangyiMCCheckworld_error.txt");
            var logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {title}\n\n{message}\n\n";
            System.IO.File.AppendAllText(logPath, logContent);
        }
        catch
        {
            Console.WriteLine($"{title}: {message}");
        }
    }
}