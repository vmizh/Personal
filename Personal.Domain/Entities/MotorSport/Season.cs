using Personal.Domain.Entities.Base;

namespace Personal.Domain.Entities.MotorSport;

/// <summary>
/// Сезон 
/// </summary>
public class Season : BaseReference
{
    public List<Tuple<RefName, RefName>> Drivers { set; get; } = new List<Tuple<RefName, RefName>>();
    public List<RefName> Races { set; get; } = new List<RefName>();
}
