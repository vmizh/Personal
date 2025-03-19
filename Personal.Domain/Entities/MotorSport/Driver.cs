using Personal.Domain.Entities.Base;

namespace Personal.Domain.Entities.MotorSport;

/// <summary>
/// Гонщик
/// </summary>
public class Driver : BaseReference
{
    public string? FullName { set; get; }
    public DateTime Birthday { set; get; }
    public string? Country { set; get; }
    public string? Bio { set; get; }
}
