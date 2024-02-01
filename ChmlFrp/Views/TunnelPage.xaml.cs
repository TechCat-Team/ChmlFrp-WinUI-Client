using ChmlFrp.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using ChmlFrp.Core.Models;
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.AppNotifications;
using Windows.Storage.Streams;
using System.Text;

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

    public class TunnelStatus
    {
        public string Name
        {
            get; set;
        } = "";
        public bool IsActive
        {
            get; set;
        }
        public Process? Process
        {
            get; set; 
        }
        public string Output
        {
            get; set;
        } = "";
    }

    public static Dictionary<int, TunnelStatus> tunnelStatus = new();

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
                var isSwitchOn = toggleSwitch.IsOn;
                // 如果操作为打开toggleWitch
                if (isSwitchOn)
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
                        tunnelStatus[tunnelInfo.TunnelId].IsActive = progressBar.IsIndeterminate = toggleSwitch.IsOn;

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
                            var Command = $"frpc -u {UserInfo.Get.UserToken} -p {tunnelId}";

                            // 创建 ProcessStartInfo 对象来配置进程启动信息
                            ProcessStartInfo psi = new ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                RedirectStandardInput = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WorkingDirectory = folderPath,
                                StandardOutputEncoding = Encoding.UTF8,
                                //Arguments = Command
                            };

                            // 使用Task.Run在新的任务中并行执行异步操作
                            await Task.Run(async () =>
                            {
                                // 启动进程
                                using (Process process = new Process { StartInfo = psi })
                                {
                                    tunnelStatus[tunnelInfo.TunnelId].Process = process;
                                    process.Start();
                                    process.StandardInput.WriteLine(Command);

                                    using (CancellationTokenSource cts = new CancellationTokenSource())
                                    {
                                        Task<string> outputTask = Task.Run(async () =>
                                        {
                                            StringBuilder outputBuilder = new StringBuilder();
                                            var lineCount = 0;

                                            // 异步读取输出流
                                            while (!process.StandardOutput.EndOfStream)
                                            {
                                                var line = await process.StandardOutput.ReadLineAsync();

                                                // 隐藏usertoken
                                                line = line.Replace(UserInfo.Get.UserToken, string.Empty);

                                                // 隐藏前4行内容
                                                if (lineCount < 4)
                                                {
                                                    lineCount++;
                                                    continue;
                                                }

                                                // 显示其他内容
                                                outputBuilder.AppendLine(line);

                                                // 在每次读取输出后更新outputTask
                                                outputTask = Task.FromResult(outputBuilder.ToString());
                                                var processedOutput = await outputTask;
                                                var utf8Bytes = Encoding.UTF8.GetBytes(processedOutput);
                                                tunnelStatus[tunnelInfo.TunnelId].Output = Encoding.UTF8.GetString(utf8Bytes);
                                                tunnelStatus[tunnelId].Output = Encoding.UTF8.GetString(utf8Bytes);
                                            }

                                            return outputBuilder.ToString();
                                        }, cts.Token);

                                        // 等待6秒
                                        await Task.Delay(6000);

                                        // 检查输出是否包含指定字样
                                        var finalOutput = await outputTask;
                                        if (finalOutput.Contains("映射启动成功"))
                                        {
                                            progressBar.ShowError = false;
                                            progressBar.ShowPaused = false;
                                            progressBar.IsIndeterminate = false;
                                            progressBar.Value = 100;
                                        }
                                        process.WaitForExit();
                                    }
                                }
                            });
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                   tunnelStatus[tunnelInfo.TunnelId].Process.Kill();
                   tunnelStatus[tunnelInfo.TunnelId].Process.Dispose();
                   tunnelStatus[tunnelInfo.TunnelId].Process = null;
                }
            }
        }
    }
    //static async Task<bool> WaitForSuccess(Task<string> outputTask, TimeSpan timeout, int tunnelId)
    //{
    //    DateTime startTime = DateTime.Now;

    //    while ((DateTime.Now - startTime) < timeout)
    //    {
    //        await Task.Delay(1000);

    //        var processedOutput = await outputTask;
    //        var utf8Bytes = Encoding.UTF8.GetBytes(processedOutput);
    //        tunnelStatus[tunnelId].Output = Encoding.UTF8.GetString(utf8Bytes);
    //        // 在异步任务未完成时检查输出
    //        if (outputTask.IsCompleted)
    //        {
    //            var output = await outputTask;
    //            if (output.Contains("映射启动成功"))
    //            {
    //                return true;
    //            }
    //        }
    //    }

    //    return false;
    //}

    private void toggleSwitch_Loaded(object sender, RoutedEventArgs e)
    {
        var toggleSwitch = ((ToggleSwitch)sender);
        if (toggleSwitch != null)
        {
            // 获取与ToggleSwitch关联的TunnelInfo对象
            TunnelInfo? tunnelInfo = (TunnelInfo)toggleSwitch.DataContext;

            if (tunnelInfo != null)
            {
                tunnelStatus.TryAdd(tunnelInfo.TunnelId, new TunnelStatus() { IsActive = false, Name = tunnelInfo.TunnelName});
                toggleSwitch.IsOn = tunnelStatus[tunnelInfo.TunnelId].IsActive;
                ProgressBar? progressBar = toggleSwitch.Tag as ProgressBar;

                if (progressBar != null && toggleSwitch.IsOn)
                {
                    progressBar.Value = 100;
                }
            }

            toggleSwitch.Toggled += ToggleSwitch_Toggled;
        }

    }

}
