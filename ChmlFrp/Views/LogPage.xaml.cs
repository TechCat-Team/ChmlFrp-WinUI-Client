using ChmlFrp.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace ChmlFrp.Views;

public sealed partial class LogPage : Page
{
    public LogViewModel ViewModel
    {
        get;
    }

    public LogPage()
    {
        ViewModel = App.GetService<LogViewModel>();
        InitializeComponent();
    }
}
