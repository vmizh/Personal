using Personal.WPFClient.Repositories.Base;
using WPFClient.Configuration;

namespace Personal.WPFClient.Repositories;

public class CountryRepository : BaseRepository<Domain.Entities.Country>, ICountryRepository
{
    public CountryRepository(ServiceConfigurationBuilder config) : base(config)
    {
        Endpoint = "Country";
    }
}
