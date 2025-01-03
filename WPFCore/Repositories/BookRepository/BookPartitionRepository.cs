using Personal.WPFClient.Repositories.Base;
using WPFClient.Configuration;

namespace Personal.WPFClient.Repositories;

public class BookPartitionRepository : BaseRepository<Domain.Entities.BookPartition>, IBookPartitionRepository
{
    public BookPartitionRepository(ServiceConfigurationBuilder config) : base(config)
    {
        Endpoint = "BookPartition";
    }
}
