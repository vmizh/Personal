using Personal.Data.Repositories;
using Personal.Domain.Entities;

namespace Personal.Services.Services;

public class BookPartitionsService(IBaseRepository<BookPartition> repository) : BaseService<BookPartition>(repository), IBookPartitionsService
{
    protected override string RepositoryName => "Репозиторий разделов";
}
