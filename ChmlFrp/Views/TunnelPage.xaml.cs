using ChmlFrp.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using ChmlFrp.Core.Models;
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.AppNotifications;

namespace ChmlFrp.Views;

public sealed partial class TunnelPage : Page
{
    public TunnelViewModel ViewModel
    {
        get;
    }

    public TunnelPage()
    {
        ViewModel = App.GetService<TunnelViewModel>();
        InitializeComponent();
    }

    private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        // 获取ToggleSwitch实例
        ToggleSwitch? toggleSwitch = sender as ToggleSwitch;

        if (toggleSwitch != null)
        {
            // 获取与ToggleSwitch关联的TunnelInfo对象
            TunnelInfo? tunnelInfo = (TunnelInfo)toggleSwitch.DataContext;

            if (tunnelInfo != null)
            {
                // 获取对应的TunnelId
                var tunnelId = tunnelInfo.TunnelId;
                // 获取对应的NodeInfo
                var nodeinfo = tunnelInfo.NodeInfo;
                // 获取对应的TunnelName
                var tunnelname = tunnelInfo.TunnelName;
                // 获取对应的LinkAddress
                var linkaddress = tunnelInfo.LinkAddress;
                // 获取对应的IntranetInfo
                var intranetinfo = tunnelInfo.IntranetInfo;
                // 获取ToggleSwitch关联的ProgressBar实例
                ProgressBar? progressBar = toggleSwitch.Tag as ProgressBar;

                if (progressBar != null)
                {
                    // 根据ToggleSwitch的状态设置ProgressBar的IsIndeterminate属性
                    progressBar.IsIndeterminate = toggleSwitch.IsOn;

                    var folderName = "frpc";
                    var executableName = "frpc.exe";

                    // 获取当前应用程序的基础目录
                    var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var folderPath = Path.Combine(currentDirectory, folderName);

                    // 检查frp文件是否存在
                    var executablePath = Path.Combine(folderPath, executableName);
                    if (File.Exists(executablePath))
                    {
                        // 组装简易启动指令
                        var Command = $"frpc.exe -u {UserInfo.UserToken} -p {tunnelId}";

                        // 创建 ProcessStartInfo 对象来配置进程启动信息
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WorkingDirectory = folderPath
                        };

                        // 启动进程
                        using (Process process = new Process { StartInfo = psi })
                        {
                            process.Start();

                            // 将cmd命令写入标准输入流
                            process.StandardInput.WriteLine(Command);
                            process.StandardInput.Flush();
                            process.StandardInput.Close();

                            // 异步读取输出流
                            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();

                            // 等待10秒或直到输出中包含"映射启动成功"
                            var success = await WaitForSuccess(outputTask, TimeSpan.FromSeconds(10));

                            if (success)
                            {
                                progressBar.ShowError = false;
                                progressBar.ShowPaused = false;
                                progressBar.IsIndeterminate = false;
                                progressBar.Value = 100;
                                var builder = new AppNotificationBuilder()
                                    .AddText("映射启动成功");

                                var notificationManager = AppNotificationManager.Default;
                                notificationManager.Show(builder.BuildNotification());
                            }
                            else
                            {
                                progressBar.IsIndeterminate = true;
                                progressBar.ShowError = true;
                                var builder = new AppNotificationBuilder()
                                    .AddText("映射启动失败")
                                    .AddText("详细信息请前往日志页面查看");

                                var notificationManager = AppNotificationManager.Default;
                                notificationManager.Show(builder.BuildNotification());
                            }

                            process.WaitForExit();
                        }
                    }
                    else
                    {
                    
                    }
                }
            }
        }
    }
    static async Task<bool> WaitForSuccess(Task<string> outputTask, TimeSpan timeout)
    {
        DateTime startTime = DateTime.Now;

        while ((DateTime.Now - startTime) < timeout)
        {
            await Task.Delay(100);

            // 在异步任务未完成时检查输出
            if (outputTask.IsCompleted)
            {
                var output = await outputTask;
                if (output.Contains(UserInfo.UserToken))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
