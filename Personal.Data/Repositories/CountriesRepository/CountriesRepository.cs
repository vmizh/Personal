using Personal.Domain.Config;
using Personal.Domain.Entities;

namespace Personal.Data.Repositories;

public class CountriesRepository(MongoDBContext dbContext) : BaseRepository<Country>(dbContext), ICountriesRepository
{
    
}
