using Personal.Domain.Entities.Base;

namespace Personal.Domain.Entities;

public class Book : BaseReference
{
    public List<RefName>? AuthorList { get; set; } = [];
    public int? PublisherYear { set; get; }
    public string? Publisher { set; get; }
    public string? Annotation { set; get; }
    public string? TableOfContents { set; get; }
    public List<RefName>? Genres { set; get; }
}
