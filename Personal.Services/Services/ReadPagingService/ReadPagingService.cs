using Personal.Data.Repositories;
using Personal.Domain.Entities;

namespace Personal.Services.Services;

public class ReadPagingService(IBaseRepository<ReadPaging> repository) : BaseService<ReadPaging>(repository), IReadPagingService
{
    protected override string RepositoryName => "Репозиторий чтения по датам и страницам";
}
