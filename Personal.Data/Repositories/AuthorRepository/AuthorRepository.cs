using Personal.Domain.Config;
using Personal.Domain.Entities;

namespace Personal.Data.Repositories;

public class AuthorRepository(MongoDBContext dbContext) : BaseRepository<Author>(dbContext), IAuthorRepository
{
}
