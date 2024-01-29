using ChmlFrp.Contracts.Services;
using ChmlFrp.Helpers;
using ChmlFrp.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using Windows.System;
using Windows.Storage;
using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.AppNotifications;
using Microsoft.UI.Xaml.Media.Imaging;
using ChmlFrp.Core.Models;


namespace ChmlFrp.Views;

// TODO: Update NavigationViewItem titles and icons in ShellPage.xaml.
public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel
    {
        get;
    }

    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
        // 默认不显示登录失败提示
        myInfoBar.IsOpen = false;

        DowFrpAsync();

        if (!string.IsNullOrEmpty(UserInfo.UserToken))
        {
            // 设置 TextBlock 文本的内容
            usernameTextBlock.Text = UserInfo.UserName;
            emailTextBlock.Text = UserInfo.Email;
            userImgPicture.DisplayName = UserInfo.UserName;


            var imagePath = UserInfo.UserImage;

            // 创建一个 BitmapImage 对象
            BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));

            // 将 BitmapImage 设置为 ImageSource
            userImgPicture.ProfilePicture = bitmapImage;
            userImgPictureA.ProfilePicture = bitmapImage;

            // 允许点击 Shell_Tunnel
            Shell_Tunnel.IsEnabled = true;
        }

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();
    }
    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        App.AppTitlebar = AppTitleBarText as UIElement;
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }

    private async void OnFooterButtonClick(object sender, RoutedEventArgs e)
    {
        // 如果app.usertoken没有数据(没有登录数据)，则允许点击块
        if (string.IsNullOrEmpty(UserInfo.UserToken))
        {
            myProgressBar.Visibility = Visibility.Collapsed;
            // 加载一言
            LoadTextFromApi();
            // 在按钮点击时，显示 ContentDialog
            ContentDialogResult result = await loginContentDialog.ShowAsync();

            // 处理 ContentDialog 返回的结果
            if (result == ContentDialogResult.Primary)
            {
                // 用户点击了"确定"按钮
                myProgressBar.Visibility = Visibility.Visible;
                var username = UsernameTextBox.Text;
                var password = PasswordBox.Password;
                // 检测用户名和密码是否为空
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    myInfoBar.IsOpen = true;
                    myInfoBar.Message = "用户名或密码不能为空";
                    myProgressBar.ShowPaused = true;
                }
                // 如果不为空 继续执行指令
                else
                {
                    using HttpClient httpClient = new HttpClient();
                    // 构建请求的URL
                    var apiUrl = $"https://panel.chmlfrp.cn/api/login.php?username={username}&password={password}";

                    try
                    {
                        // 发送GET请求
                        HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                        // 检查响应是否成功
                        if (response.IsSuccessStatusCode)
                        {
                            // 读取响应内容
                            var responseContent = await response.Content.ReadAsStringAsync();

                            // 解析JSON响应
                            // 使用 Newtonsoft.Json 库
                            dynamic apiResult = Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent);

                            // 检查返回的 code
                            if (apiResult.code != null)
                            {
                                // 获取用户数据
                                // 用户名
                                string returnedUsername = apiResult.username;
                                // 用户头像
                                string userImg = apiResult.userimg;
                                // 用户token
                                string userToken = apiResult.token;
                                // 实名状态
                                string realname = apiResult.realname;
                                // 限速
                                string bandwidth = apiResult.bandwidth;
                                // 限制隧道
                                string tunnel = apiResult.tunnel;
                                // 当前隧道使用数
                                string tunnelstate = apiResult.tunnelstate;
                                // 当前积分
                                string integral = apiResult.integral;
                                // 权限组到期时间
                                string term = apiResult.term;
                                // 权限组
                                string usergroup = apiResult.usergroup;
                                // qq
                                string qq = apiResult.qq;
                                // 邮箱
                                string email = apiResult.email;
                                // 用户id
                                string userid = apiResult.userid;


                                // 如果userimg不为空,则缓存头像图片。
                                if (!string.IsNullOrEmpty(userImg))
                                {
                                    // 下载并缓存用户图标
                                    var localImagePath = await CacheUserImage(userImg, returnedUsername);
                                    UserInfo.UserImage = localImagePath;
                                    // 构建通知
                                    var builder = new AppNotificationBuilder()
                                    .AddText($"欢迎{returnedUsername}！")
                                    .AddText("ChmlFrp登录成功！")
                                    .SetAppLogoOverride(new Uri($"{localImagePath}"), AppNotificationImageCrop.Circle);

                                    var notificationManager = AppNotificationManager.Default;
                                    notificationManager.Show(builder.BuildNotification());
                                }
                                else
                                {
                                    // 构建通知
                                    var builder = new AppNotificationBuilder()
                                    .AddText($"欢迎{returnedUsername}！")
                                    .AddText("ChmlFrp登录成功！");

                                    var notificationManager = AppNotificationManager.Default;
                                    notificationManager.Show(builder.BuildNotification());
                                }
                                // 如果用户勾选了自动登录。
                                if (AutoLoginCheckBox.IsChecked == true)
                                {
                                    // 使用本地缓存保存用户信息
                                    ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

                                    localSettings.Values["Username"] = returnedUsername;
                                    localSettings.Values["UserImg"] = UserInfo.UserImage;
                                    localSettings.Values["UserToken"] = userToken;
                                    localSettings.Values["Realname"] = realname;
                                    localSettings.Values["Bandwidth"] = bandwidth;
                                    localSettings.Values["Tunnel"] = tunnel;
                                    localSettings.Values["TunnelState"] = tunnelstate;
                                    localSettings.Values["Integral"] = integral;
                                    localSettings.Values["Term"] = term;
                                    localSettings.Values["UserGroup"] = usergroup;
                                    localSettings.Values["QQ"] = qq;
                                    localSettings.Values["Email"] = email;
                                    localSettings.Values["UserID"] = userid;
                                }

                                // 保存用户信息到 App 属性中
                                UserInfo.UserName = returnedUsername;
                                UserInfo.UserToken = userToken;
                                UserInfo.RealName = realname;
                                UserInfo.Bandwidth = bandwidth;
                                UserInfo.Tunnel = tunnel;
                                UserInfo.TunnelState = tunnelstate;
                                UserInfo.Integral = integral;
                                UserInfo.Term = term;
                                UserInfo.UserGroup = usergroup;
                                UserInfo.QQ = qq;
                                UserInfo.Email = email;
                                UserInfo.UserId = userid;

                                // 允许点击 Shell_Tunnel
                                Shell_Tunnel.IsEnabled = true;
                                // 设置 TextBlock 的文本为 App.LoggedInUsername 的内容
                                usernameTextBlock.Text = UserInfo.UserName;
                                emailTextBlock.Text = UserInfo.Email;
                                userImgPicture.DisplayName = UserInfo.UserName;

                                var imagePath = UserInfo.UserImage;

                                // 创建一个 BitmapImage 对象
                                BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));

                                // 将 BitmapImage 设置为 ImageSource
                                userImgPicture.ProfilePicture = bitmapImage;
                                userImgPictureA.ProfilePicture = bitmapImage;
                            }
                            else
                            {
                                // 获取错误信息
                                string error = apiResult.error;
                                myInfoBar.IsOpen = true;
                                myInfoBar.Message = error;
                                myProgressBar.ShowPaused = true;
                            }
                        }
                        else
                        {
                            myInfoBar.IsOpen = true;
                            myInfoBar.Message = "https请求失败，请检查网络连接";
                        }
                    }
                    catch (Exception ex)
                    {
                        // 处理异常
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
            }
        }
    }

    private async void LoadTextFromApi()
    {
        var apiUrl = "https://uapis.cn/api/say";

        using HttpClient client = new HttpClient();
        try
        {
            // 异步获取 API 内容
            var apiContent = await client.GetStringAsync(apiUrl);

            // 将获取的内容设置为 YYTextBlock 的 Text 属性
            YYTextBlock.Text = apiContent;
        }
        catch (Exception ex)
        {
            // 处理异常，输出异常信息
            YYTextBlock.Text = "无法加载内容：" + ex.Message;
        }
    }

    private async Task<string> CacheUserImage(string apiUserImg, string username)
    {
        // 获取本地文件夹
        StorageFolder localFolder = ApplicationData.Current.LocalFolder;

        // 构建本地文件路径
        var fileName = $"{username}_userimg.jpg";
        StorageFile localFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

        // 使用 HttpClient 下载用户图标并保存到本地文件
        using (HttpClient httpClient = new HttpClient())
        {
            var imageBytes = await httpClient.GetByteArrayAsync(apiUserImg);

            using Stream stream = await localFile.OpenStreamForWriteAsync();
            await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
        }

        // 返回本地文件的路径
        return localFile.Path;
    }

    // 点击登出
    private void Log_out(object sender, RoutedEventArgs e)
    {
        ClearLocalSettings();
    }

    private void ClearLocalSettings()
    {
        // 获取本地设置对象
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        // 列举的本地缓存信息的键
        string[] keysToClear = {
        "Username",
        "UserImg",
        "UserToken",
        "Realname",
        "Bandwidth",
        "Tunnel",
        "TunnelState",
        "Integral",
        "Term",
        "UserGroup",
        "QQ",
        "Email",
        "UserID"
        };

        // 清除每个键对应的值
        foreach (var key in keysToClear)
        {
            if (localSettings.Values.ContainsKey(key))
            {
                localSettings.Values.Remove(key);
            }
        }


        UserInfo.UserName = null;
        UserInfo.UserImage = null;
        UserInfo.UserToken = null;
        UserInfo.RealName = null;
        UserInfo.Bandwidth = null;
        UserInfo.Tunnel = null;
        UserInfo.TunnelState = null;
        UserInfo.Integral = null;
        UserInfo.Term = null;
        UserInfo.UserGroup = null;
        UserInfo.QQ = null;
        UserInfo.Email = null;
        UserInfo.UserId = null;

        // 设置 TextBlock 的文本为 App.LoggedInUsername 的内容
        usernameTextBlock.Text = "您尚未登录";
        emailTextBlock.Text = "点击此块以登录";

        // 构建通知
        var builder = new AppNotificationBuilder()
        .AddText("成功登出账户")
        .AddText("您的账户已退出登录");

        var notificationManager = AppNotificationManager.Default;
        notificationManager.Show(builder.BuildNotification());

        // 不允许点击 Shell_Tunnel
        Shell_Tunnel.IsEnabled = false;
        //Environment.Exit(0);
    }

    private async Task DowFrpAsync()
    {
        var folderName = "frpc";
        var executableName = "frpc.exe";

        // 获取当前应用程序的基础目录
        var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var folderPath = Path.Combine(currentDirectory, folderName);

        // 检查文件夹是否存在
        if (!Directory.Exists(folderPath))
        {
            // 如果文件夹不存在则创建文件夹
            Directory.CreateDirectory(folderPath);
        }

        // 检查文件是否存在
        var executablePath = Path.Combine(folderPath, executableName);
        if (!File.Exists(executablePath))
        {
            Download_frpc.IsOpen = true;

            try
            {
                // 创建 Progress 对象，用于接收下载进度
                var progress = new Progress<int>(value =>
            {
                // 更新 ProgressBar 的值
                Download_ProgressBar.Value = value;
                if (value == 100)
                {
                    // 下载成功，关闭Download_frpc
                    Download_frpc.IsOpen = false;
                }
            });

                // 下载文件并自动更新进度到ProgressBar
                var architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString();
                if (architecture == "X64")
                {
                    Download_frpc.Subtitle = "未检测到frpc软件，frpc_amd64正在下载中";
                    await DownloadFileAsync("https://chmlfrp.cn/dw/windows/amd64/frpc.exe", executablePath, progress);
                }
                else if (architecture == "Arm64")
                {
                    Download_frpc.Subtitle = "未检测到frpc软件，frpc_arm64正在下载中";
                    await DownloadFileAsync("https://chmlfrp.cn/dw/windows/arm64/frpc.exe", executablePath, progress);
                }
                else if (architecture == "X86")
                {
                    Download_frpc.Subtitle = "未检测到frpc软件，frpc_386正在下载中";
                    await DownloadFileAsync("https://chmlfrp.cn/dw/windows/386/frpc.exe", executablePath, progress);
                }
                else if (architecture == "Arm")
                {
                    Download_frpc.Subtitle = "未检测到frpc软件，frpc_arm64正在下载中";
                    await DownloadFileAsync("https://chmlfrp.cn/dw/windows/arm64/frpc.exe", executablePath, progress);
                }
                else
                {
                    Download_frpc.Subtitle = "未检测到frpc软件，frpc_386正在下载中";
                    await DownloadFileAsync("https://chmlfrp.cn/dw/windows/386/frpc.exe", executablePath, progress);
                }
            }
            catch (HttpRequestException ex)
            {
                Download_ProgressBar.IsIndeterminate = true;
                Download_ProgressBar.ShowError = true;
                Download_frpc.Title = "HTTP 请求异常";
                Download_frpc.Subtitle = ex.Message;
            }
            catch (TaskCanceledException ex)
            {
                Download_ProgressBar.IsIndeterminate = true;
                Download_ProgressBar.ShowPaused = true;
                Download_frpc.Title = "任务取消异常";
                Download_frpc.Subtitle = ex.Message;
            }
            catch (Exception ex)
            {
                Download_ProgressBar.IsIndeterminate = true;
                Download_ProgressBar.ShowError = true;
                Download_frpc.Title = "下载失败";
                Download_frpc.Subtitle = ex.Message;
            }
        }
    }

    static async Task DownloadFileAsync(string url, string filePath, IProgress<int> progress)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(new Uri(url), HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
        var buffer = new byte[8192];
        var totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead);
            totalBytesRead += bytesRead;

            // 报告进度
            progress.Report((int)((double)totalBytesRead / response.Content.Headers.ContentLength * 100));
        }
    }

    private void OnPaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
    {
        ButtonOnPaneOpened.Visibility = Visibility.Collapsed;
        ButtonOnPaneClosing.Visibility = Visibility.Visible;
    }

    private void OnPaneOpened(NavigationView sender, object args)
    {
        ButtonOnPaneOpened.Visibility = Visibility.Visible;
        ButtonOnPaneClosing.Visibility = Visibility.Collapsed;
    }

}
