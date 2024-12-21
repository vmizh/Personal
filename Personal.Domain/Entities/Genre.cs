using Personal.Domain.Entities.Base;

namespace Personal.Domain.Entities;

public class Genre : BaseReference
{
    public Guid? ParentId { set; get; }
    public RefName? Partition { set; get; }
}
