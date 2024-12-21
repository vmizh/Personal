using Personal.Domain.Config;
using Personal.Domain.Entities;

namespace Personal.Data.Repositories;

public class ReadPagingRepository(MongoDBContext dbContext) : BaseRepository<ReadPaging>(dbContext), IReadPagingRepository
{
    
}
