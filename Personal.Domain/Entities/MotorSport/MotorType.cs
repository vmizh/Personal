using System.ComponentModel.DataAnnotations;

namespace Personal.Domain.Entities.MotorSport;

public enum MotorType
{
    [Display(Name = "Автомобиль")] Auto = 0,
    [Display(Name = "Мотоцикл")] Motorbike = 1
}
