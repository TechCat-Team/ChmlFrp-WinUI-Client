﻿using System.Text;
using ChmlFrp.ViewModels;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using WinRT.Interop;

namespace ChmlFrp.Views;

public sealed partial class LogPage : Page
{
    private DispatcherTimer timer;

    public LogViewModel ViewModel
    {
        get;
    }

    public LogPage()
    {
        ViewModel = App.GetService<LogViewModel>();
        InitializeComponent();
        foreach (var t in TunnelPage.tunnelStatus)
        {
            ActiveTunnelsCombo.Items.Add(t.Key);
        }

        // Initialize and start the timer
        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private void Timer_Tick(object sender, object e)
    {
        // Update the log every second
        UpdateLog();
    }

    private void UpdateLog()
    {
        if (ActiveTunnelsCombo.SelectedItem != null)
        {
            logRtb.Blocks.Clear();
            var tunnelId = Convert.ToInt32(ActiveTunnelsCombo.SelectedItem.ToString());
            // Create a RichTextBlock, a Paragraph and a Run.
            Paragraph paragraph = new Paragraph();
            Run run = new Run();

            // Customize some properties on the RichTextBlock.
            logRtb.IsTextSelectionEnabled = true;
            logRtb.TextWrapping = TextWrapping.Wrap;

            // Convert the string to UTF-8
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(TunnelPage.tunnelStatus[tunnelId].Output);
            string utf8String = Encoding.UTF8.GetString(utf8Bytes);

            run.Text = utf8String;

            // Add the Run to the Paragraph, the Paragraph to the RichTextBlock.
            paragraph.Inlines.Add(run);
            logRtb.Blocks.Add(paragraph);
        }
    }

    private void ClearButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        logRtb.Blocks.Clear();
    }

    private async void SaveButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ActiveTunnelsCombo.SelectedItem != null)
        {
            logRtb.SelectAll();
            File.WriteAllText(ActiveTunnelsCombo.SelectedItem.ToString() + ".log", logRtb.SelectedText);
            logRtb.Select(logRtb.SelectionStart, logRtb.SelectionStart);
        }
    }

    private void ActiveTunnelsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Update the log when the selection changes
        UpdateLog();
    }
}