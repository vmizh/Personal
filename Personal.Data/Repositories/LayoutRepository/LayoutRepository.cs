using Personal.Domain.Config;
using Personal.Domain.Entities;

namespace Personal.Data.Repositories;

public class LayoutRepository(LayoutContext dbContext) : BaseLayoutRepository<Layout>(dbContext), ILayoutRepository
{
}
