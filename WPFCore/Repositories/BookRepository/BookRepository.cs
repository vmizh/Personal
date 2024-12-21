using Personal.WPFClient.Repositories.Base;
using WPFClient.Configuration;

namespace Personal.WPFClient.Repositories;

public class BookRepository : BaseRepository<Domain.Entities.Book>, IBookRepository
{
    public BookRepository(ServiceConfigurationBuilder config) : base(config)
    {
        Endpoint = "Book";
    }
}
