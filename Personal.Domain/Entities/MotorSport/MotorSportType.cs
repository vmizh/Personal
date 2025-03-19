using System.Diagnostics;
using Personal.Domain.Entities.Base;

namespace Personal.Domain.Entities.MotorSport;

[DebuggerDisplay("'{_id}' Name")]
public class MotorSportType : BaseReference
{
    public MotorType MotorType { set; get; }
}
