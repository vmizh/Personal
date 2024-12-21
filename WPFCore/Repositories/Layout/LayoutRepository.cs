using Personal.WPFClient.Repositories.Base;
using WPFClient.Configuration;

namespace Personal.WPFClient.Repositories.Layout;

public class LayoutRepository : BaseRepository<Domain.Entities.Layout>, ILayoutRepository
{
    public LayoutRepository(ServiceConfigurationBuilder config) : base(config)
    {
        Endpoint = "Layout";
    }
}
