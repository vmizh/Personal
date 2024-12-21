using Personal.Data.Repositories;
using Personal.Domain.Entities;

namespace Personal.Services.Services;

public class BookService(IBaseRepository<Book> repository) : BaseService<Book>(repository), IBookService
{
    protected override string RepositoryName => "Репозиторий книг";
}
