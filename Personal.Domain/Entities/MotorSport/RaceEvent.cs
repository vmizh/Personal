using Personal.Domain.Entities.Base;

namespace Personal.Domain.Entities.MotorSport;

/// <summary>
/// Событие
/// </summary>
public class RaceEvent : BaseReference
{
    public required RefName RaceTrack { set; get; }
    public required DateTime Start { set; get; }
}
