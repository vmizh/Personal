using Personal.Domain.Entities.Base;

namespace Personal.Domain.Entities;

public class ReadPaging : BaseReference
{ 
    public RefName? Book { set; get; }
    public DateTime? Readed { set; get; }
    public required List<ReadPage> Read { set; get; } = [];
   
   
}
