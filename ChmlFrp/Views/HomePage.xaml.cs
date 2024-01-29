using System.Net.NetworkInformation;
using ChmlFrp.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using ChmlFrp.Core.Models;

namespace ChmlFrp.Views;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel
    {
        get;
    }

    public HomePage()
    {
        ViewModel = App.GetService<HomeViewModel>();
        InitializeComponent();
        SomeMethod();
        DateTime currentTime = DateTime.Now;
        var greetingText = GetGreeting(currentTime.Hour);

        timeHelloTextBlock.Text = greetingText;

        static string GetGreeting(int hour)
        {
            if (hour >= 0 && hour < 6)
            {
                return "夜深了！";
            }
            else if (hour >= 6 && hour < 11)
            {
                return "早上好！";
            }
            else if (hour >= 11 && hour < 14)
            {
                return "中午好！";
            }
            else if (hour >= 14 && hour < 15)
            {
                return "饮茶先啦！";
            }
            else if (hour >= 15 && hour < 17)
            {
                return "下午好！";
            }
            else if (hour >= 17 && hour < 22)
            {
                return "晚上好！";
            }
            else
            {
                return "少熬夜噢！";
            }
        }

        if (!string.IsNullOrEmpty(UserInfo.UserToken))
        {
            // 设置 TextBlock 文本的内容
            usernameTextBlock.Text = UserInfo.UserName;
            userImgPicture.DisplayName = UserInfo.UserName;
            integralTextBlock.Text = UserInfo.Integral + "积分";
            userid.Text = "用户ID：" + UserInfo.UserId;
            username.Text = "用户名：" + UserInfo.UserName;
            email.Text = "邮箱号：" + UserInfo.Email;
            usergroup.Text = "权限组：" + UserInfo.UserGroup;
            tunnel.Text = "隧道数：" + UserInfo.TunnelState + " / " + UserInfo.Tunnel + "条";
            var bandwidthw = 4 * int.Parse(UserInfo.Bandwidth);
            bandwidth.Text = "限带宽：" + UserInfo.Bandwidth + "m | " + bandwidthw + "m";
            realname.Text = "实名状态：" + UserInfo.RealName;

            var imagePath = UserInfo.UserImage;

            // 创建一个 BitmapImage 对象
            BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));

            // 将 BitmapImage 设置为 ImageSource
            userImgPicture.ProfilePicture = bitmapImage;
        }
    }
    private void SomeMethod()
    {
        // 模拟按钮点击事件
        RoutedEventArgs args = new RoutedEventArgs();
        YanchiButton_Click(YanchiButton_Click, args);
    }

    private async void YanchiButton_Click(object sender, RoutedEventArgs e)
    {
        // 执行ping操作并更新TextBlock的内容
        var pingResult = await PingAsync("panel.chmlfrp.cn");
        yanchiTextBlock.Text = pingResult;
    }

    private async Task<string> PingAsync(string hostNameOrAddress)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(hostNameOrAddress);
            if (reply.Status == IPStatus.Success)
            {
                // 返回ping的延迟时间
                return reply.RoundtripTime.ToString() + "ms";
            }
            else
            {
                lianjieTextBlock.Text = "连接API失败";
                return "无法连接";
            }
        }
        catch (Exception ex)
        {
            lianjieTextBlock.Text = "发生错误";
            return ex.Message;
        }
    }

    private void GoWenJuan(object sender, RoutedEventArgs e)
    {
        // 打开链接的代码
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://wj.qq.com/s2/14070072/f1ab/",
            UseShellExecute = true
        });
    }
    private void ToGoQQOne(object sender, RoutedEventArgs e)
    {
        // 打开链接的代码
        Process.Start(new ProcessStartInfo
        {
            FileName = "http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=G7h0hsrOzquMR0gjK0A2mMoolEiEcZfj&authKey=2rzIx4iehaFN5GCRx4AD4U7aVPH9idBo4%2F5Uef6e5N6Amr%2BQxcRv39KYjdkj2%2Ff8&noverify=0&group_code=992067118",
            UseShellExecute = true
        });
    }
    private void ToGoQQTwo(object sender, RoutedEventArgs e)
    {
        // 打开链接的代码
        Process.Start(new ProcessStartInfo
        {
            FileName = "http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=g3q2td6MA0P48gbl-ZB6H7FxK0RRG1Pw&authKey=v9qQ5KbeyZ2eJxWZqFcMpG5sptj1nVzlJBkY3MkINyAFYuGg2pBsvMUTtXk%2FIJ8d&noverify=0&group_code=592908249",
            UseShellExecute = true
        });
    }

}
