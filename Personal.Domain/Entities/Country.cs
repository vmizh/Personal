using System.ComponentModel.DataAnnotations;
using Personal.Domain.Entities.Base;

namespace Personal.Domain.Entities;

public class Country : BaseReference
{
    public bool Deleted { get; set; } = false;
    
    [MaxLength(2)]
    public string? Sign2 { get; set; }
    [MaxLength(3)]
    public string? Sign3 { get; set; }
    public int? Iso { get; set; }
    [MaxLength(200)]
    public string? ForeignName { get; set; }
}
