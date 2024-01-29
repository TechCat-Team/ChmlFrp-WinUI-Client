using System.Reflection;
using System.Security.Principal;
using ChmlFrp.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;

namespace ChmlFrp.Views;

public sealed partial class SettingsPage : Page
{
    private const string RegistryKeyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
    private const string AppName = "ChmlFrp";

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
        // 查询注册表，获取开机自启状态
        var isAutoStartEnabled = IsAutoStartEnabled();

        // 设置PowerBootToggleSwitch的状态
        PowerBootToggleSwitch.IsOn = isAutoStartEnabled;

        // 添加PowerBootToggleSwitch的事件处理程序
        PowerBootToggleSwitch.Toggled += YourToggleSwitch_Toggled;
    }

    private void YourToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        // 获取当前进程的 Windows 身份标识
        WindowsIdentity identity = WindowsIdentity.GetCurrent();

        // 创建 WindowsPrincipal 对象，用于检查用户权限
        WindowsPrincipal principal = new WindowsPrincipal(identity);

        // 检查用户是否是管理员
        var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

        if (!isAdmin)
        {
            // 如果不是管理员，则显示 ContentDialog 提示
            ShowAdminDialog();
        }else
        {
        // 获取ToggleSwitch的最新状态
        var isAutoStartEnabled = PowerBootToggleSwitch.IsOn;

        // 根据ToggleSwitch的状态设置开机自启动
        SetAutoStart(isAutoStartEnabled);
        }
    }

    private async void ShowAdminDialog()
    {
        // 在按钮点击时，显示 ContentDialog
        ContentDialogResult result = await TQContentDialog.ShowAsync();

        // 处理 ContentDialog 返回的结果
        if (result == ContentDialogResult.Primary)
        {
            // 退出应用程序
            Application.Current.Exit();
        }
        else
        {

        }
    }

    private void SetAutoStart(bool enable)
    {
        using RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
        if (key != null)
        {
            if (enable)
            {
                // 获取应用程序的可执行文件路径
                var appPath = Assembly.GetExecutingAssembly().Location;

                // 设置开机自启动
                key.SetValue(AppName, appPath);
            }
            else
            {
                // 取消开机自启动
                key.DeleteValue(AppName, false);
            }
        }
    }

    private bool IsAutoStartEnabled()
    {
        using RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
        if (key != null)
        {
            // 检查注册表中是否存在对应的键值
            return key.GetValue(AppName) != null;
        }

        return false;
    }

    public SettingsViewModel ViewModel
    {
        get;
    }
}
