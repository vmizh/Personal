using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Mvvm;
using DevExpress.Xpf.Editors.Settings;
using Personal.Domain.Entities;
using Personal.Domain.Entities.Base;
using Personal.WPFClient.Helper.Window;
using Personal.WPFClient.Repositories;
using Personal.WPFClient.Repositories.Base;
using Personal.WPFClient.Repositories.Layout;
using Personal.WPFClient.Views;
using Personal.WPFClient.Wrappers;
using WPFClient.Configuration;
using WPFCore.Repositories;
using WPFCore.ViewModel;
using WPFCore.Window.Base;
using WPFCore.Window.Properties;
using WPFCore.Wrappers;
using Application = System.Windows.Application;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace Personal.WPFClient.ViewModels;

public class BooksWindowViewModel : ViewModelWindowBase
{
    private readonly IAuthorRepository myAuthorRepository;
    private readonly IBookRepository myBookRepository;
    private readonly IGenreRepository myGenreRepository;

    public BooksWindowViewModel(IAuthorRepository authorRepository, IBookRepository bookRepository,
        ILayoutRepository layoutRepository, IGenreRepository genreRepository)
    {
        myAuthorRepository = authorRepository;
        myBookRepository = bookRepository;
        myGenreRepository = genreRepository;
        myLayoutRepository = layoutRepository;
        DataControl = new BooksView();
        Properties.WindowTitle = "Список книг";
        Properties.FormNameProperty = new FormNameProperty
        {
            FormName = "Персональная база",
            FormNameColor = new SolidColorBrush(Colors.Green)
        };
        Properties.Id = MenuAndDocumentIds.BookMenuId;
        Properties.LeftMenuBar =
        [
            new MenuButtonInfo
            {
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuOptions"] as ControlTemplate,
                ToolTip = "Настройки",
                SubMenu =
                [
                    new MenuButtonInfo
                    {
                        Image = Application.Current.Resources["imageResetLayout"] as DrawingImage,
                        Caption = "Переустановить разметку",
                        Command = OnWindowResetLayoutCommand
                    }
                ]
            }
        ];
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
                Content = Application.Current.Resources["menuDocumentOpen"] as ControlTemplate,
                ToolTip = "Открыть выбранный документ",
                Command = DocumentOpenCommand
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
        AddCommand = new DelegateCommand(
            AddBook,
            () => true);
        DeleteCommand = new DelegateCommand(
            DeleteBook,
            () => CurrentBook != null);
        UpdateCommand = new DelegateCommand(
            EditBook, CurrentBook is not null);
        LoadReferences();
    }


    #region Properties

    public ObservableCollection<BookWrapper> Books { set; get; } = [];
    public ObservableCollection<BookWrapper> SelectedBooks { set; get; } = [];
    public ObservableCollection<BookWrapper> DeletedBooks { set; get; } = [];

    public List<RefName> Authors { set; get; } = [];

    public BookWrapper CurrentBook
    {
        get => GetValue<BookWrapper>();
        set => SetValue(value);
    }

    #endregion

    #region Commands

    public ICommand UpdateCommand { get; private set; }

    private void EditBook()
    {
        var doc = new BookCardViewModel(myAuthorRepository, myBookRepository, myLayoutRepository, myGenreRepository);
        doc.LoadDocument(CurrentBook); 
        doc.Show().ConfigureAwait(true);
        doc.IsOpening = false;
    }
    public ICommand AddCommand { get; private set; }
    public ICommand DeleteCommand { get; private set; }

    private void AddBook()
    {
        var newItem = new BookWrapper(new Book
        {
            _id = Guid.NewGuid(),
            Name = null,
            AuthorList = [],
            Genres = []
        })
        {
            State = StateEnum.New
        };
        Books.Add(newItem);
        CurrentBook = newItem;
        var doc = new BookCardViewModel(myAuthorRepository, myBookRepository, myLayoutRepository, myGenreRepository);
        doc.LoadDocument(CurrentBook);
        doc.Show().ConfigureAwait(true);
    }

    private void DeleteBook()
    {
        DeletedBooks.Add(CurrentBook);
        Books.Remove(CurrentBook);
    }

    public void LoadReferences()
    {
        Authors.Clear();
        var auths = Task.Run(() => ((BaseRepository<Author>)myAuthorRepository).GetAllAsync()).Result;
        if (auths is null) return;
        foreach (var auth in auths.OrderBy(_ => _.LastName))
            Authors.Add(new RefName
            {
                Id = auth._id,
                Name = auth.LastName + (!string.IsNullOrWhiteSpace(auth.FirstName)
                                         ? $" {auth.FirstName.First()}."
                                         : string.Empty)
                                     + (!string.IsNullOrWhiteSpace(auth.SecondName)
                                         ? $"{auth.SecondName.First()}."
                                         : string.Empty)
            });
    }

    public override bool CanDocumentOpen => CurrentBook != null;

    public override async Task DocumentOpenAsync()
    {
        var doc = new BookCardViewModel(myAuthorRepository, myBookRepository, myLayoutRepository, myGenreRepository);
        doc.LoadDocument(CurrentBook);
        await doc.Show();
    }

    public override async Task DocumentRefreshAsync()
    {
        try
        {
            if (FormWindow is not null)
                FormWindow.loadingIndicator.Visibility = Visibility.Visible;
            var data = await ((BaseRepository<Book>)myBookRepository).GetAllAsync();
            Books.Clear();
            if (data is not null)
                foreach (var auth in data.OrderBy(_ => _.Name))
                    Books.Add(new BookWrapper(auth)
                    {
                        AuthorAllList = Authors,
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

    public override bool CanDocumentSave => Books != null && ((Books.Count > 0 &&
                                                               Books.Any(_ => _.State != StateEnum.NotChanged) &&
                                                               Books.All(_ => !string.IsNullOrWhiteSpace(_.Name))) ||
                                                              DeletedBooks.Any());

    public override async Task DocumentSaveAsync()
    {
        try
        {
            if (DeletedBooks.Any())
                foreach (var book in DeletedBooks)
                    await ((BaseRepository<Book>)myBookRepository).DeleteAsync(book.Model);
            foreach (var book in Books.Where(_ => _.State != StateEnum.NotChanged))
                switch (book.State)
                {
                    case StateEnum.Changed:
                        await ((BaseRepository<Book>)myBookRepository).UpdateAsync(book.Model);
                        break;
                    case StateEnum.New:
                        await ((BaseRepository<Book>)myBookRepository).CreateAsync(book.Model);
                        break;
                }

            DeletedBooks.Clear();
            foreach (var book in Books) book.State = StateEnum.NotChanged;
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    protected override async Task OnWindowLoadedAsync()
    {
        await base.OnWindowLoadedAsync();
        if (DataControl is BooksView view)
        {
            view.GridControl.Columns.GetColumnByFieldName("TableOfContents").Visible = false;
            var ann = view.GridControl.Columns.GetColumnByFieldName("Annotation");
            if (ann is not null)
            {
                ann.EditSettings = new MemoEditSettings()
                {
                    ShowIcon = false,
                    TextWrapping = TextWrapping.NoWrap
                };
            }
        }
    }

    #endregion
}
