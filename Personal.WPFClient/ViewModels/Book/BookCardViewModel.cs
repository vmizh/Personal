using System;
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
using Personal.WPFClient.Views.Book;
using Personal.WPFClient.Wrappers;
using WPFClient.Configuration;
using WPFCore.Repositories;
using WPFCore.ViewModel;
using WPFCore.Window.Base;
using WPFCore.Window.Properties;
using WPFCore.Wrappers;

namespace Personal.WPFClient.ViewModels;

public class BookCardViewModel : ViewModelWindowBase
{
    private readonly IAuthorRepository myAuthorRepository;
    private readonly IBookRepository myBookRepository;
    private readonly IGenreRepository myGenreRepository;
    private RefName myCurrentAutor;
    private RefName myCurrentGenre;

    public BookCardViewModel(IAuthorRepository authorRepository, IBookRepository bookRepository,
        ILayoutRepository layoutRepository, IGenreRepository genreRepository)
    {
        myBookRepository = bookRepository;
        myGenreRepository = genreRepository;
        myAuthorRepository = authorRepository;
        myLayoutRepository = layoutRepository;
        DataControl = new BookCardView();
        Properties.WindowTitle = "Книга ...";
        Properties.FormNameProperty = new FormNameProperty
        {
            FormName = "Персональная база",
            FormNameColor = new SolidColorBrush(Colors.Green)
        };
        Properties.Id = MenuAndDocumentIds.BookDocumentId;
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

        AddAuthorCommand = new DelegateCommand(AddAuthor, () => true);
        DeleteAuthorCommand = new DelegateCommand(
            DeleteAuthor,
            () => CurrentAuthor != null);
        AddGenreCommand = new DelegateCommand(AddGenre, () => true);
        DeleteGenreCommand = new DelegateCommand(
            DeleteGenre,
            () => CurrentGenre != null);
        //DocumentChangedCommand = new DelegateCommand<object>(
        //    DocumentChanged,
        //    o => true);
    }


    public BookWrapper Document { get; set; }

    public RefName CurrentAuthor
    {
        get => myCurrentAutor;
        set
        {
            if (Equals(value, myCurrentAutor)) return;
            myCurrentAutor = value;
            RaisePropertyChanged(nameof(CurrentAuthor));
        }
    }

    public RefName CurrentGenre
    {
        get => myCurrentGenre;
        set
        {
            if (Equals(value, myCurrentGenre)) return;
            myCurrentGenre = value;
            RaisePropertyChanged(nameof(CurrentGenre));
        }
    }

    private void DocumentChanged(object o)
    {
        if (Document.State == StateEnum.New) return;
        Document.State = StateEnum.Changed;
    }

    private void DeleteGenre()
    {
        // ReSharper disable once PossibleNullReferenceException
        Document.Genres.Remove(CurrentGenre);
        if (DataControl is BookCardView view)
            view.gridGenres.RefreshData();
        if (Document.State != StateEnum.New)
            Document.State = StateEnum.Changed;
    }

    private void DeleteAuthor()
    {
        // ReSharper disable once PossibleNullReferenceException
        Document.AuthorList.Remove(CurrentAuthor);
        if (DataControl is BookCardView view)
            view.gridAuthors.RefreshData();
        if (Document.State != StateEnum.New)
            Document.State = StateEnum.Changed;
    }

    private void AddAuthor()
    {
        var ctx = new AuthorsDialogViewModel(myAuthorRepository, myLayoutRepository);
        var service = GetService<IDialogService>("DialogServiceUI");
        if (service.ShowDialog(MessageButton.OKCancel, "Запрос", ctx) == MessageResult.Cancel) return;
        var auth = ctx.CurrentAuthor;
        if (auth == null) return;
        if (Document.AuthorList.Any(_ => _.Id == auth.Id)) return;
        Document.AuthorList.Add(new RefName
        {
            Id = auth.Id,
            Name = auth.LastName + (!string.IsNullOrWhiteSpace(auth.FirstName)
                                     ? $" {auth.FirstName.First()}."
                                     : string.Empty)
                                 + (!string.IsNullOrWhiteSpace(auth.SecondName)
                                     ? $"{auth.SecondName.First()}."
                                     : string.Empty)
        });

        if (DataControl is BookCardView view)
            view.gridAuthors.RefreshData();
        if (Document.State != StateEnum.New)
            Document.State = StateEnum.Changed;
    }

    private void AddGenre()
    {
        var ctx = new GenreSelectDialogViewModel(myGenreRepository, myLayoutRepository);
        var service = GetService<IDialogService>("DialogServiceUI");
        if (service.ShowDialog(MessageButton.OKCancel, "Запрос", ctx) == MessageResult.Cancel) return;
        if (!ctx.SelectedGenres.Any()) return;
        foreach (var genre in ctx.SelectedGenres)
        {
            if (Document.Genres.Any(_ => _.Id == genre.Id)) continue;
            Document.Genres.Add(new RefName
            {
                Id = genre.Id,
                Name = genre.Name
            });
        }

        if (DataControl is BookCardView view)
            view.gridGenres.RefreshData();
        if (Document.State != StateEnum.New)
            Document.State = StateEnum.Changed;
    }

    public void LoadDocument(BookWrapper doc)
    {
        Document = doc;
        Properties.WindowTitle = $"Книга: {Document.Name}";
    }

    public async Task LoadDocumentAsync(Guid id)
    {
        Document = await ((BaseRepository<BookWrapper>)myBookRepository).GetByIdAsync(id);
        Properties.WindowTitle = $"Книга: {Document.Name}";
    }

    #region Commands

    public override Task DocumentRefreshAsync()
    {
        return base.DocumentRefreshAsync();
    }

    public ICommand<object> AddAuthorCommand { get; private set; }
    public ICommand DeleteAuthorCommand { get; private set; }
    public ICommand AddGenreCommand { get; private set; }
    public ICommand DeleteGenreCommand { get; private set; }
    public ICommand<object> DocumentChangedCommand { get; private set; }


    public override bool CanDocumentSave => Document != null && !string.IsNullOrWhiteSpace(Document.Name) &&
                                            Document.State != StateEnum.NotChanged;

    public override async Task DocumentSaveAsync()
    {
        Document.TableOfContents = ((BookCardView)DataControl).tableOfContentsEditor.Text;
        Document.TableOfContentsRtf = string.IsNullOrWhiteSpace(Document.TableOfContents)
            ? null
            : ((BookCardView)DataControl).tableOfContentsEditor.RtfText;
        try
        {
            switch (Document.State)
            {
                case StateEnum.New:
                    await ((BaseRepository<Book>)myBookRepository).CreateAsync(Document.Model);
                    break;
                case StateEnum.Changed:
                    await ((BaseRepository<Book>)myBookRepository).UpdateAsync(Document.Model);
                    break;
            }

            Document.State = StateEnum.NotChanged;
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    #endregion
}
