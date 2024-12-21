using DevExpress.Xpf.Core;
using WPFClient.Configuration;

namespace Personal.WPFClient;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : ThemedWindow
{
    private readonly IServiceConfiguration myServiceConfig;
   

    public MainWindow(ServiceConfigurationBuilder serviceConfig)
    {
        myServiceConfig = serviceConfig.Config;
        InitializeComponent();
        //DataContext = new MainViewModel(serviceConfig);
    }
}
