using Personal.Domain.Config;
using Personal.Domain.Entities;

namespace Personal.Data.Repositories;

public class BookPartitionsRepository(MongoDBContext dbContext) : BaseRepository<BookPartition>(dbContext), IBookPartitionsRepository
{
}
