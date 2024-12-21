using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Mvvm;
using Personal.Domain.Entities;
using Personal.Domain.Entities.Base;
using Personal.WPFClient.Helper.Window;
using Personal.WPFClient.Repositories;
using Personal.WPFClient.Repositories.Base;
using Personal.WPFClient.Repositories.Layout;
using Personal.WPFClient.Views.ReadPaging;
using Personal.WPFClient.Wrappers.Base;
using Personal.WPFClient.Wrappers.ReadPagingWrapper;
using WPFClient.Configuration;
using WPFCore.ViewModel;
using WPFCore.Window.Base;
using WPFCore.Window.Properties;

namespace Personal.WPFClient.ViewModels.ReadPaging;

public class ReadPagingViewModel : ViewModelWindowBase
{
    private readonly IBookRepository myBookRepository;
    private readonly IReadPagingRepository myReadPagingRepository;

    public ReadPagingViewModel(IBookRepository bookRepository, IReadPagingRepository readPagingRepository,
        ILayoutRepository layuotRepository)
    {
        myBookRepository = bookRepository;
        myReadPagingRepository = readPagingRepository;
        myLayoutRepository = layuotRepository;
        DataControl = new ReadPagingView();
        Properties.WindowTitle = "Процесс чтения";
        Properties.FormNameProperty = new FormNameProperty
        {
            FormName = "Персональная база",
            FormNameColor = new SolidColorBrush(Colors.Green)
        };
        Properties.Id = MenuAndDocumentIds.ReadPagingMenuId;
        Properties.RightMenuBar =
        [
            new MenuButtonInfo
            {
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                ToolTip = "Обновить",
                Command = DocumentRefreshCommand
            },
            new MenuButtonInfo
            {
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                ToolTip = "Сохранить",
                Command = DocumentSaveCommand
            },
            new MenuButtonInfo
            {
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                ToolTip = "Закрыть документ",
                Command = DocumentCloseCommand
            }
        ];
        AddBookCommand = new DelegateCommand(
            AddBook,
            () => true);
        ChangeBookCommand = new DelegateCommand(
            ChangeBook,
            () => CurrentReadPaging is not null);
        DeleteBookCommand = new DelegateCommand(
            DeleteBook,
            () => CurrentReadPaging is not null);

        AddPageCommand = new DelegateCommand(
            AddPage,
            () => true);

        DeletePageCommand = new DelegateCommand(
            DeletePage,
            () => CurrentRead is not null);
    }


    #region Properties

    public ObservableCollection<ReadPagingWrapper> PageReadings { set; get; } = [];
    public ObservableCollection<ReadPagingWrapper> SelectedPageReadings { set; get; } = [];

    public ObservableCollection<ReadPageWrapper> Reads { set; get; } = [];

    public ObservableCollection<ReadPagingWrapper> DeletedPageReadings { set; get; } = [];

    public ObservableCollection<ReadPageWrapper> SelectedReads { set; get; } = [];

    public ReadPagingWrapper CurrentReadPaging
    {
        get => GetValue<ReadPagingWrapper>();
        set
        {
            SetValue(value);
            Reads.Clear();
            if (CurrentReadPaging is null) return;
            foreach (var r in CurrentReadPaging.Read)
            {
                Reads.Add(r);
            }
        }
    }

    public ReadPageWrapper CurrentRead
    {
        get => GetValue<ReadPageWrapper>();
        set => SetValue(value);
    }

    #endregion

    #region Commands

    public override async Task Show()
    {
        await DocumentRefreshAsync();
        await base.Show();
    }

    public override async Task DocumentRefreshAsync()
    {
        try
        {
            if (FormWindow is not null)
                FormWindow.loadingIndicator.Visibility = Visibility.Visible;
            var data = await ((BaseRepository<Domain.Entities.ReadPaging>)myReadPagingRepository).GetAllAsync();
            PageReadings.Clear();
            if (data is not null)
                // ReSharper disable once PossibleNullReferenceException
                foreach (var p in data.OrderBy(_ => _.Book.Name))
                    PageReadings.Add(new ReadPagingWrapper(p)
                    {
                        State = StateEnum.NotChanged
                    });
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
        finally
        {
            if (FormWindow is not null)
                FormWindow.loadingIndicator.Visibility = Visibility.Hidden;
        }
    }

    public override bool CanDocumentSave =>
        DeletedPageReadings.Any() || PageReadings.Any(_ => _.State != StateEnum.NotChanged);

    public override async Task DocumentSaveAsync()
    {
        try
        {
            if (DeletedPageReadings.Any())
                foreach (var p in DeletedPageReadings)
                    await ((BaseRepository<Domain.Entities.ReadPaging>)myReadPagingRepository).DeleteAsync(p.Model);
            foreach (var p in PageReadings.Where(_ => _.State != StateEnum.NotChanged))
                switch (p.State)
                {
                    case StateEnum.Changed:
                        await ((BaseRepository<Domain.Entities.ReadPaging>)myReadPagingRepository).UpdateAsync(p.Model);
                        break;
                    case StateEnum.New:
                        await ((BaseRepository<Domain.Entities.ReadPaging>)myReadPagingRepository).CreateAsync(p.Model);
                        break;
                }

            DeletedPageReadings.Clear();
            foreach (var p in PageReadings)
            {
                foreach (var pp in p.Read) pp.State = StateEnum.NotChanged;
                p.State = StateEnum.NotChanged;
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    public ICommand AddBookCommand { get; private set; }

    public ICommand ChangeBookCommand { get; private set; }

    public ICommand DeleteBookCommand { get; private set; }

    public ICommand AddPageCommand { get; private set; }
    public ICommand DeletePageCommand { get; private set; }

    private void DeletePage()
    {
       
        CurrentReadPaging.Model.Read.Remove(CurrentRead.Model);
        CurrentReadPaging.Read.Remove(CurrentRead); 
        Reads.Remove(CurrentRead);
        if (CurrentReadPaging.State != StateEnum.New)
            CurrentReadPaging.State = StateEnum.Changed;
    }

    private void AddPage()
    {
        var newPage = new ReadPageWrapper(new ReadPage
        {
            Date = DateTime.Today
        }, CurrentReadPaging);
        CurrentReadPaging.Read.Add(newPage);
        Reads.Add(newPage);
        CurrentRead = newPage;
        if (CurrentReadPaging.State != StateEnum.New)
            CurrentReadPaging.State = StateEnum.Changed;
        if (DataControl is ReadPagingView view)
        {
            view.TableViewPages.MoveLastRow();
            view.gridPages.CurrentColumn = view.gridPages.Columns[0];
        }
    }

    private void DeleteBook()
    {
        DeletedPageReadings.Add(CurrentReadPaging);
        PageReadings.Remove(CurrentReadPaging);
        if (CurrentReadPaging.State != StateEnum.New)
            CurrentReadPaging.State = StateEnum.Changed;
    }

    private void AddBook()
    {
        var ctx = new BooksDialogViewModel(myBookRepository);
        var service = GetService<IDialogService>("DialogServiceUI");
        if (service.ShowDialog(MessageButton.OKCancel, "Запрос", ctx) == MessageResult.Cancel) return;
        var book = ctx.CurrentBook;
        if (book == null) return;
        var newRead = new ReadPagingWrapper(new Domain.Entities.ReadPaging
        {
            _id = Guid.NewGuid(),
            Book = new RefName
            {
                Id = book.Id,
                Name = book.Name
            },
            Read = []
        })
        {
            Name = book.Authors
        };
        PageReadings.Add(newRead);
        if (DataControl is ReadPagingView view)
        {
            view.TableViewBooks.MoveLastRow();
            view.gridBooks.CurrentColumn = view.gridBooks.Columns[0];
        }
        CurrentReadPaging = newRead;

        if (CurrentReadPaging.State != StateEnum.New)
            CurrentReadPaging.State = StateEnum.Changed;
    }

    private void ChangeBook()
    {
        var ctx = new BooksDialogViewModel(myBookRepository);
        var service = GetService<IDialogService>("DialogServiceUI");
        if (service.ShowDialog(MessageButton.OKCancel, "Запрос", ctx) == MessageResult.Cancel) return;
        var book = ctx.CurrentBook;
        if (book == null) return;
        CurrentReadPaging.Book = new RefName
        {
            Id = book.Id,
            Name = book.Name
        };
        CurrentReadPaging.Name = book.Authors;
    }

    #endregion
}
