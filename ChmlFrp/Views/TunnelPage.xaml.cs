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
using Windows.ApplicationModel.DataTransfer;
using System.Net.NetworkInformation;
using System.Net;
using Windows.Services.Maps;

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
                            var Command = $"-u {UserInfo.Get.UserToken} -p {tunnelId}";

                            // 创建 ProcessStartInfo 对象来配置进程启动信息
                            ProcessStartInfo psi = new ProcessStartInfo
                            {
                                FileName = executablePath, // 完整的 frpc.exe 文件路径
                                Arguments = Command, // 传递组装的指令作为参数
                                RedirectStandardInput = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WorkingDirectory = folderPath,
                                StandardOutputEncoding = Encoding.UTF8
                            };

                            // 使用Task.Run在新的任务中并行执行异步操作
                            await Task.Run(async () =>
                            {
                                // 启动进程
                                using (Process process = new Process { StartInfo = psi })
                                {
                                    tunnelStatus[tunnelInfo.TunnelId].Process = process;
                                    process.Start();

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
                    tunnelStatus[tunnelInfo.TunnelId].IsActive = false;
                    if (tunnelStatus[tunnelInfo.TunnelId].Process != null)
                    {
                        try
                        {
                            tunnelStatus[tunnelInfo.TunnelId].Process.Kill();
                            tunnelStatus[tunnelInfo.TunnelId].Process.Dispose();
                        }
                        catch (Exception ex)
                        {
                        }
                        finally
                        {
                            tunnelStatus[tunnelInfo.TunnelId].Process = null;
                        }
                    }
                }
            }
        }
    }

    private void ShareButton_Click(object sender, RoutedEventArgs e)
    {
        // 获取AppBarButton实例
        AppBarButton? appBarButton = sender as AppBarButton;

        if (appBarButton != null)
        {
            // 获取与ToggleSwitch关联的TunnelInfo对象
            TunnelInfo? tunnelInfo = (TunnelInfo)appBarButton.DataContext;

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
                var parts = linkaddress.Split(':'); // 分离域名和端口

                if (parts.Length == 2)
                {
                    var domain = parts[0];
                    int port;

                    if (int.TryParse(parts[1], out port))
                    {
                        // 查询DNS解析获取节点ip
                        IPAddress ipAddress = GetIpAddress(domain);

                        if (ipAddress != null)
                        {
                            Console.WriteLine($"域名: {domain}");
                            Console.WriteLine($"端口: {port}");
                            Console.WriteLine($"IP 地址: {ipAddress}");

                            var textToCopy = $@"ChmlFrp - 映射信息
------------
隧道信息：#{tunnelId} {tunnelname}
节点信息：{nodeinfo}
连接地址：{linkaddress}
------------
如果出现无法连接的情况，可使用IP连接(不推荐)：{ipAddress}:{port}";
                            CopyTextToClipboard(textToCopy);
                        }
                        else
                        {
                            var textToCopy = $@"ChmlFrp - 映射信息
------------
隧道信息：#{tunnelId} {tunnelname}
节点信息：{nodeinfo}
连接地址：{linkaddress}
------------
获取节点IP失败，DNS可能尚未刷新，此错误可能会导致映射无法正常运行";
                            CopyTextToClipboard(textToCopy);
                        }
                    }
                    else
                    {
                        Console.WriteLine("端口号不是有效的数字。");
                    }
                }
                else
                {
                    Console.WriteLine("连接地址格式错误。");
                }
            }
        }
    }

    private void CopyTextToClipboard(string text)
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(text);
        Clipboard.SetContent(dataPackage);
    }

    static IPAddress GetIpAddress(string domain)
    {
        IPAddress[] addresses = Dns.GetHostAddresses(domain);
        if (addresses.Length > 0)
            {
                return addresses[0];
            }
            else
            {
                return null;
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
