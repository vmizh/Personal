using Personal.Data.Repositories;
using Personal.Domain.Entities;

namespace Personal.Services.Services;

public class CountriesService(IBaseRepository<Country> repository) : BaseService<Country>(repository), ICountriesService
{
    protected override string RepositoryName => "Репозиторий стран";
}
