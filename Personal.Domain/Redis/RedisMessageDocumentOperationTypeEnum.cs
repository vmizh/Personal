using System.ComponentModel.DataAnnotations;

namespace Personal.Domain.Redis;

public enum RedisMessageDocumentOperationTypeEnum
{
    [Display(Name = "Открытие")]
    Open = 0,
    [Display(Name = "Создание")]
    Create = 1,
    [Display(Name = "Обновление")]
    Update = 2,
    [Display(Name = "Удаление")]
    Delete = 3,
    [Display(Name = "Выполнение")]
    Execute = 4,
    [Display(Name = "Не определен")]
    NotDefined= 5,
}
