using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using DevExpress.Mvvm.UI.Native;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Personal.WPFClient.Helper;
using Personal.WPFClient.Helper.Window;
using Personal.WPFClient.Repositories;
using Personal.WPFClient.Repositories.Layout;
using Personal.WPFClient.ViewModels;
using Personal.WPFClient.ViewModels.ReadPaging;
using Serilog;
using WPFClient.Configuration;

namespace Personal.WPFClient;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;
    
    static App()
    {
        CompatibilitySettings.UseLightweightThemes = true;
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("Logs/Log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    public App()
    {
        _host = new HostBuilder().ConfigureServices((context, services) =>
            {
                services.AddSingleton<ServiceConfigurationBuilder>();
                services.AddSingleton<MainViewModel>();
                services.AddScoped<IAuthorRepository, AuthorRepository>();
                services.AddScoped<ICountryRepository, CountryRepository>();
                services.AddScoped<IBookRepository, BookRepository>();
                services.AddScoped<IReadPagingRepository, ReadPagingRepository>();
                services.AddScoped<ILayoutRepository, LayoutRepository>();
                services.AddScoped<AuthorsWindowViewModel>();
                services.AddScoped<ReadPagingViewModel>();
            })
            .Build();
        var ci = new CultureInfo("ru-RU");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}" +
                              "\\KursAM2v3"))
            Directory.CreateDirectory(
                $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\KursAM2v3");
        Current.Properties.Add("DataPath",
            $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\KursAM2v3");
        Dispatcher.UnhandledException += OnDispatcherUnhandledException;
    }

    protected async void OnStartup(object sender, StartupEventArgs startupEventArgs)
    {
        try
        {
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            ToolTipService.ShowOnDisabledProperty.OverrideMetadata(
                typeof(Control<>),
                new FrameworkPropertyMetadata(true));
            GridControlLocalizer.Active = new CustomDXGridLocalizer();
            EditorLocalizer.Active = new CustomEditorsLocalizer();
            DXMessageBoxLocalizer.Active = new CustomDXMessageBoxLocalizer();

            EventManager.RegisterClassHandler(typeof(GridColumn), DXSerializer.AllowPropertyEvent,
                new AllowPropertyEventHandler((_, e) =>
                {
                    if (!e.Property.Name.Contains("Header")) return;
                    e.Allow = false;
                    e.Handled = true;
                }));

            await _host.StartAsync();

            _host.Services.GetService<MainViewModel>();
            
        }
        catch (Exception ex)
        {
            WindowManager.ShowKursDialog(ex.Message, "Ошибка!",
                new SolidColorBrush(Colors.Red),
                WindowManager.KursDialogResult.Confirm);
            Log.Logger.Error(ex, ex.Message);
        }
    }

    private void OnDispatcherUnhandledException(object sender,
        DispatcherUnhandledExceptionEventArgs e)
    {
        var sb = new StringBuilder(e.Exception.Message);
        var ex1 = e.Exception.InnerException;
        while (ex1 != null) sb.Append($"\n{ex1.Message}");
        MessageBox.Show("Unhandled exception occurred: \n" + e.Exception.Message, "Error", MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception.Message);
        MessageBox.Show("Не предвиденная ошибка, обратитьсь к администратору."
                        + Environment.NewLine + e.Exception.Message, "Непредвиденная ошибка");

        e.Handled = true;
    }

    private async void Application_Exit(object sender, ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }
    }
}
