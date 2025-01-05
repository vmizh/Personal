using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;
using Personal.Domain.Entities;
using Personal.Domain.Redis;
using Personal.WPFClient.Document;
using Personal.WPFClient.Helper.Window;
using Personal.WPFClient.Repositories;
using Personal.WPFClient.Repositories.Base;
using Personal.WPFClient.Repositories.Layout;
using Personal.WPFClient.Views;
using Personal.WPFClient.Wrappers.Menu;
using Serilog;
using ServiceStack.Redis;
using WPFClient.Configuration;
using WPFCore.Repositories;
using WPFCore.ViewModel;
using WPFCore.Window.Base;
using WPFCore.Window.Properties;
using WindowStartupLocation = Personal.Domain.Helper.WindowStartupLocation;
using WindowState = Personal.Domain.Helper.WindowState;

namespace Personal.WPFClient.ViewModels;

public class MainViewModel : ViewModelWindowBase
{
    private readonly IAuthorRepository myAuthorRepository;
    private readonly IBookRepository myBookRepository;
    private readonly ICountryRepository myCountryRepository; 
    private readonly IReadPagingRepository myReadPagingRepository;
    private readonly IBookPartitionRepository myBookPartRepository;
    private readonly IGenreRepository myGenreRepository;

    private readonly DocumentOpen myDocumentOpen;
    private readonly ServiceConfigurationBuilder myServiceConfig;
    private readonly RedisManagerPool redisManager;

    public MainViewModel(ServiceConfigurationBuilder serviceConfig, IAuthorRepository authorRepository,
        ICountryRepository countryRepository, IBookRepository bookRepository,
        IReadPagingRepository readPagingRepository,
        ILayoutRepository layoutRepository, IBookPartitionRepository bookPartRepository, IGenreRepository genreRepository)
    {
        Properties.Name = "Персональная база";
        Properties.Id = Guid.Parse("{3378BDFE-F66B-4B39-B5A6-016DF88FEC3A}");
        Properties.FormNameProperty = new FormNameProperty
        {
            FormName = "Персональная база",
            FormNameColor = new SolidColorBrush(Colors.Green)
        };

        Properties.WindowTitle = "Главное окно";
        myServiceConfig = serviceConfig;
        myAuthorRepository = authorRepository;
        myCountryRepository = countryRepository;
        myBookRepository = bookRepository;
        myReadPagingRepository = readPagingRepository;
        myBookPartRepository = bookPartRepository;
        myGenreRepository = genreRepository;
        myLayoutRepository = layoutRepository;
        myDocumentOpen = new DocumentOpen(myAuthorRepository, myCountryRepository, myBookRepository,
            myReadPagingRepository, myLayoutRepository, myBookPartRepository,myGenreRepository);
        redisManager = new RedisManagerPool(myServiceConfig.Config.RedisCache.ConnectionString);
        ThreadPool.QueueUserWorkItem(_ =>
        {
            using (var redisClient = redisManager.GetClient())
            {
                using (var subscription = redisClient.CreateSubscription())
                {
                    subscription.OnSubscribe = channel => { Console.WriteLine($"Client #{channel} Subscribed"); };
                    subscription.OnUnSubscribe = channel => { Console.WriteLine($"Client #{channel} UnSubscribed"); };
                    subscription.OnMessage = OnMessage;

                    subscription.SubscribeToChannels("Menu");
                }
            }
        });


        DataControl = new MainView();
        Menus.Add(new MainMenuWrapper(redisManager)
        {
            Name = "Авторы",
            Id = MenuAndDocumentIds.AuthorMenuId,
            Title = "Авторы"
        });
        Menus.Add(new MainMenuWrapper(redisManager)
        {
            Name = "Книги",
            Id = MenuAndDocumentIds.BookMenuId,
            Title = "Книги"
        });
        Menus.Add(new MainMenuWrapper(redisManager)
        {
            Name = "Чтение",
            Id = MenuAndDocumentIds.ReadPagingMenuId,
            Title = "Чтение"
        });
        Menus.Add(new MainMenuWrapper(redisManager)
        {
            Name = "Разделы литературы",
            Id = MenuAndDocumentIds.BookPartitionMenuId,
            Title = "Разделы (физика, худ.лит-ра, история ..."
        });
        Menus.Add(new MainMenuWrapper(redisManager)
        {
            Name = "Тип литературы",
            Id = MenuAndDocumentIds.GenreMenuId,
            Title = "Тип литератур, например Термодинамика, Юмор(сатира), Скорочтение..."
        });

        Log.Information("Загрузка главного окна");
        FormWindow = new FormWindowBase
        {
            DataContext = this
        };
        FormWindow.Show();
        FormWindow.Closed += FormWindow_Closed;
        try
        {
            if (LayoutSerializationService is not null && myLayoutRepository is not null)
            {
                Properties.DefaultLayoutString = LayoutSerializationService.Serialize();
                var l = Task.Run(() => ((BaseRepository<Layout>)myLayoutRepository).GetByIdAsync(Properties.Id)).Result;

                if (l is null) return;

                FormWindow.Height = l.FormHeight;
                FormWindow.Width = l.FormWidth;
                FormWindow.Left = l.FormLeft;
                FormWindow.Top = l.FormTop;
                FormWindow.WindowStartupLocation = l.FormStartLocation switch
                {
                    WindowStartupLocation.CenterOwner => System.Windows.WindowStartupLocation.CenterOwner,
                    WindowStartupLocation.CenterScreen => System.Windows.WindowStartupLocation.CenterScreen,
                    WindowStartupLocation.Manual => System.Windows.WindowStartupLocation.Manual,
                    _ => FormWindow.WindowStartupLocation
                };
                FormWindow.WindowState = l.FormState switch
                {
                    WindowState.Minimized => System.Windows.WindowState.Minimized,
                    WindowState.Maximized => System.Windows.WindowState.Maximized,
                    WindowState.Normal => System.Windows.WindowState.Normal,
                    _ => FormWindow.WindowState
                };
                LayoutSerializationService.Deserialize(l.LayoutString);
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }


    #region Properties

    public ObservableCollection<MainMenuWrapper> Menus { set; get; } = new ObservableCollection<MainMenuWrapper>();

    #endregion

    private void FormWindow_Closed(object sender, EventArgs e)
    {
        // ReSharper disable once RedundantNameQualifier
        var opened = new List<Window>(Helper.Windows.OpenedWindow);
        foreach (var win in opened) win.Close();
    }

    private void OnMessage(string channel, string msg)
    {
        if (string.IsNullOrWhiteSpace(msg)) return;
        var message = JsonConvert.DeserializeObject<RedisMessage>(msg);
        switch (channel)
        {
            case "Menu":
                Application.Current.Dispatcher.Invoke(
                    () => { myDocumentOpen.Open(Guid.Parse((string)message.ExternalValues["MenuId"])); });
                break;
        }
    }
}
