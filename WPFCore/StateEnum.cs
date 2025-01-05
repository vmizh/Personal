using System.ComponentModel.DataAnnotations;

namespace Personal.WPFClient.Wrappers;

/// <summary>
/// Состояние записи
/// </summary>
public enum StateEnum
{

    [Display(Name = "Сохранен")] NotChanged = 1,
    [Display(Name = "Изменен")] Changed = 2,
    [Display(Name = "Удален")] Deleted = -1,
    [Display(Name = "Новый")] New = 3,
    [Display(Name = "Не определен")] NotDefinition = 0

}
