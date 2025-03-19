using Personal.Domain.Entities.Base;

namespace Personal.Domain.Entities.MotorSport;

/// <summary>
/// Команда
/// </summary>
public class RacingTeam : BaseReference
{
    /// <summary>
    /// enum MotorType
    /// </summary>
    public required RefName MotorSportType { set; get; }
    public RefName? ParentRacingTeam { set; get; }
    public DateTime CreateDate { set; get; }
    public DateTime? CloseDate { set; get; }
    public required RefName Country { set; get; }
}
