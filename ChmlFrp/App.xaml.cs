using ChmlFrp.Activation;
using ChmlFrp.Contracts.Services;
using ChmlFrp.Core.Contracts.Services;
using ChmlFrp.Core.Services;
using ChmlFrp.Models;
using ChmlFrp.Services;
using ChmlFrp.ViewModels;
using ChmlFrp.Views;

using Windows.Storage;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using ChmlFrp.Core.Models;

namespace ChmlFrp;

public partial class App : Application
{

    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<IWebViewService, WebViewService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            services.AddSingleton<ISampleDataService, SampleDataService>();
            services.AddSingleton<IFileService, FileService>();

            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<LogViewModel>();
            services.AddTransient<LogPage>();
            services.AddTransient<HelpViewModel>();
            services.AddTransient<HelpPage>();
            services.AddTransient<TunnelViewModel>();
            services.AddTransient<TunnelPage>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<HomePage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        // 查询本地缓存是否包含用户信息
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        
        if (localSettings.Values.ContainsKey("UserToken"))
        {
            // 从本地缓存获取用户信息
            var returnedUsername = (string)localSettings.Values["Username"];
            var userImg = (string)localSettings.Values["UserImg"];
            var userToken = (string)localSettings.Values["UserToken"];
            var realname = (string)localSettings.Values["Realname"];
            var bandwidth = (string)localSettings.Values["Bandwidth"];
            var tunnel = (string)localSettings.Values["Tunnel"];
            var tunnelstate = (string)localSettings.Values["TunnelState"];
            var integral = (string)localSettings.Values["Integral"];
            var term = (string)localSettings.Values["Term"];
            var usergroup = (string)localSettings.Values["UserGroup"];
            var qq = (string)localSettings.Values["QQ"];
            var email = (string)localSettings.Values["Email"];
            var userid = (string)localSettings.Values["UserID"];

            // 将用户信息保存到App属性中
            UserInfo.UserImage = userImg;
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
        }

        // 继续应用程序启动逻辑
        await App.GetService<IActivationService>().ActivateAsync(args);
    }

}
