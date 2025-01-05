using System;
using Personal.WPFClient.Repositories;
using Personal.WPFClient.Repositories.Layout;
using Personal.WPFClient.ViewModels;
using Personal.WPFClient.ViewModels.ReadPaging;
using WPFClient.Configuration;
using WPFCore.Repositories;

namespace Personal.WPFClient.Document;

public class DocumentOpen
{
    private readonly IAuthorRepository myAuthorRepository;
    private readonly ILayoutRepository myLayoutRepository;
    private readonly ICountryRepository myCountryRepository;
    private readonly IBookRepository myBookRepository;
    private readonly IReadPagingRepository myReadPagingRepository;
    private readonly IBookPartitionRepository myBookPartRepository;
    private readonly IGenreRepository myGenreRepository;

    public DocumentOpen(IAuthorRepository authorRepository, ICountryRepository countryRepository,
        IBookRepository bookRepository, IReadPagingRepository readPagingRepository, ILayoutRepository layoutRepository,
        IBookPartitionRepository bookPartRepository, IGenreRepository genreRepository)
    {
        myAuthorRepository = authorRepository;
        myCountryRepository = countryRepository;
        myBookRepository = bookRepository;
        myReadPagingRepository = readPagingRepository;
        myLayoutRepository = layoutRepository;
        myBookPartRepository = bookPartRepository;
        myGenreRepository = genreRepository;
    }

    static DocumentOpen()
    {
        
    }
    public void Open(Guid typeOpen, Guid? docId = null)
    {
        if (typeOpen == MenuAndDocumentIds.AuthorMenuId)
        {
            var win = new AuthorsWindowViewModel(myAuthorRepository,myCountryRepository,myLayoutRepository);
            win.Show();
            return;
        }
        if (typeOpen == MenuAndDocumentIds.BookMenuId)
        {
            var win = new BooksWindowViewModel(myAuthorRepository, myBookRepository,myLayoutRepository,myGenreRepository);
            win.Show();
            return;
        }
        if (typeOpen == MenuAndDocumentIds.ReadPagingMenuId)
        {
            var win = new ReadPagingViewModel(myBookRepository,myReadPagingRepository,myLayoutRepository);
            win.Show();
            return;
        }
        if (typeOpen == MenuAndDocumentIds.BookPartitionMenuId)
        {
            var win = new BookPartitionsWindowViewModel(myBookPartRepository,myLayoutRepository);
            win.Show();
            return;
        }
        if (typeOpen == MenuAndDocumentIds.GenreMenuId)
        {
            var win = new GenreWindowViewModel(myGenreRepository,myLayoutRepository,myBookPartRepository);
            win.Show();
            return;
        }
        
    }
}
