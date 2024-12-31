using Personal.WPFClient.Helper.Window;
using Personal.WPFClient.Repositories;
using Personal.WPFClient.Views;
using Personal.WPFClient.Views.Author;
using Personal.WPFClient.Views.Book;
using Personal.WPFClient.Wrappers.Base;
using Personal.WPFClient.Wrappers;
using System.Threading.Tasks;
using System;
using Personal.Domain.Entities;
using Personal.WPFClient.Repositories.Base;
using WPFCore.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;
using Personal.WPFClient.Repositories.Layout;
using WPFClient.Configuration;

namespace Personal.WPFClient.ViewModels;

public class BooksDialogViewModel : ViewModelDialogBase
{
    private readonly IBookRepository myBookRepository;

    public BooksDialogViewModel(IBookRepository bookRepository, ILayoutRepository layoutRepository)
    {
        myBookRepository = bookRepository;
        myLayoutRepository = layoutRepository;
        Properties.Id = MenuAndDocumentIds.BookSelectMenuId;
        CustomDataUserControl = new BookSelectDialogView();
        try
        {
            var data = Task.Run(() => ((BaseRepository<Book>)myBookRepository).GetAllAsync()).Result;
            Books.Clear();
            if (data is not null)
                foreach (var book in data.OrderBy(_ => _.Name))
                    Books.Add(new BookWrapper(book)
                    {
                        State = StateEnum.NotChanged
                    });
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    public ObservableCollection<BookWrapper> Books { set; get; } = [];

    public BookWrapper CurrentBook
    {
        get => GetValue<BookWrapper>();
        set => SetValue(value);
    }
}
