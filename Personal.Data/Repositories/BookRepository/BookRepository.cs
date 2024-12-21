using Personal.Domain.Config;
using Personal.Domain.Entities;

namespace Personal.Data.Repositories;

public class BookRepository(MongoDBContext dbContext) : BaseRepository<Book>(dbContext), IBookRepository
{
    
}
