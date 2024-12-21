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
using Personal.WPFClient.Views.Book;
using Personal.WPFClient.Wrappers;
using Personal.WPFClient.Wrappers.Base;
using WPFClient.Configuration;
using WPFCore.ViewModel;
using WPFCore.Window.Base;
using WPFCore.Window.Properties;

namespace Personal.WPFClient.ViewModels;

public class BookCardViewModel : ViewModelWindowBase
{
    private readonly IAuthorRepository myAuthorRepository;
    private readonly IBookRepository myBookRepository;
    private RefName myCurrentAutor;

    public BookCardViewModel(IAuthorRepository authorRepository, IBookRepository bookRepository)
    {
        myBookRepository = bookRepository;
        myAuthorRepository = authorRepository;
        DataControl = new BookCardView();
        Properties.WindowTitle = "Книга ...";
        Properties.FormNameProperty = new FormNameProperty
        {
            FormName = "Персональная база",
            FormNameColor = new SolidColorBrush(Colors.Green)
        };
        Properties.Id = MenuAndDocumentIds.BookDocumentId;
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

    private void DeleteAuthor()
    {
        // ReSharper disable once PossibleNullReferenceException
        Document.AuthorList.Remove(CurrentAuthor);
        if(DataControl is BookCardView view)
            view.gridAuthors.RefreshData();
        if (Document.State != StateEnum.New)
            Document.State = StateEnum.Changed;
    }

    private void AddAuthor()
    {
        var ctx = new AuthorsDialogViewModel(myAuthorRepository);
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
        
        if(DataControl is BookCardView view)
            view.gridAuthors.RefreshData();
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

    public override bool CanDocumentSave => Document != null && !string.IsNullOrWhiteSpace(Document.Name) &&
                                            Document.State != StateEnum.NotChanged;

    public override async Task DocumentSaveAsync()
    {
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
