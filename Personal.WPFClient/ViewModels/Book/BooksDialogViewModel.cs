using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Personal.Domain.Entities;
using Personal.WPFClient.Helper.Window;
using Personal.WPFClient.Repositories;
using Personal.WPFClient.Repositories.Base;
using Personal.WPFClient.Repositories.Layout;
using Personal.WPFClient.Views.Book;
using Personal.WPFClient.Wrappers;
using WPFClient.Configuration;
using WPFCore.ViewModel;
using WPFCore.Wrappers;

namespace Personal.WPFClient.ViewModels;

public class BooksDialogViewModel : ViewModelDialogBase
{
    private readonly IBookRepository myBookRepository;

    public BooksDialogViewModel(IBookRepository bookRepository, ILayoutRepository layoutRepository)
    {
        myBookRepository = bookRepository;
        myLayoutRepository = layoutRepository;
        Properties.Id = MenuAndDocumentIds.BookSelectId;
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

    protected override async Task OnWindowLoadedAsync()
    {
        await base.OnWindowLoadedAsync();
        if (CustomDataUserControl is BookSelectDialogView view)
        {
            view.GridControl.Columns.GetColumnByFieldName("TableOfContents").Visible = false;
            view.GridControl.Columns.GetColumnByFieldName("Annotation").Visible = false;
        }
    }
}
