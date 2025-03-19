using Personal.Domain.Entities.Base;

namespace Personal.Domain.Entities.MotorSport;

/// <summary>
/// Гоночная трасса
/// </summary>
public class RaceTrack : BaseReference
{
    public required RefName Country { set; get; }
    public required string City { set; get; }
    public byte[]? TrackImage { set; get; } 
    public string? History { set; get; }
}
