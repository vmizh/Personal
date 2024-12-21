using Personal.Data.Repositories;
using Personal.Domain.Entities;
using Personal.Services.Response;

namespace Personal.Services.Services;

public class LayoutService(IBaseLayoutRepository<Layout> repository) : BaseLayoutService<Layout>(repository), ILayoutService
{
    protected override string RepositoryName => "Репозиторий разметки";
   
}
