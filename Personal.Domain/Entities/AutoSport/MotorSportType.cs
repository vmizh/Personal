using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Personal.Domain.Entities.Base;

namespace Personal.Domain.Entities.AutoSport;

[DebuggerDisplay("'{_id}' Name")]
public class MotorSportType : BaseReference
{
    public MotorType MotorType { set; get; }
}

public enum MotorType
{
    [Display(Name = "Автомобиль")] Auto = 0,
    [Display(Name = "Мотоцикл")] Motorbike = 1
}

public class RacingTeam : BaseReference
{
    public required RefName MotorSportType { set; get; }
    public RefName? ParentMotorSportType { set; get; }
    public DateTime CreateDate { set; get; }
    public DateTime? CloseDate { set; get; }
    public required RefName Country { set; get; }
}

/// <summary>
/// Конструктор
/// </summary>
public class Constructor : BaseReference
{

}

/// <summary>
/// Двигатель
/// </summary>
public class PowerUnit : BaseReference
{

}
/// <summary>
/// Гонщик
/// </summary>
public class DriverName : BaseReference
{

}
/// <summary>
/// Сезон 
/// </summary>
public class Season : BaseReference
{
    
}

/// <summary>
/// Гоночная трасса
/// </summary>
public class RaceTrack : BaseReference
{

}

