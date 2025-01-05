using Personal.Domain.Config;
using Personal.Domain.Entities;

namespace Personal.Data.Repositories;

public class GenreRepository(MongoDBContext dbContext) : BaseRepository<Genre>(dbContext), IGenreRepository
{
}
