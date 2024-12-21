using Personal.Domain.Entities;
using Personal.WPFClient.Repositories.Base;
using WPFClient.Configuration;

namespace Personal.WPFClient.Repositories;

public class ReadPagingRepository : BaseRepository<ReadPaging>, IReadPagingRepository
{
    public ReadPagingRepository(ServiceConfigurationBuilder config) : base(config)
    {
        Endpoint = "ReadPaging";
    }
}
