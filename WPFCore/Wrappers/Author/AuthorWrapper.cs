using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Diagnostics;
using WPFCore;
using System.ComponentModel.DataAnnotations;
using Personal.Domain.Entities.Base;

namespace Personal.WPFClient.Wrappers;

[DebuggerDisplay("Id:{Id} {Name}")]
public class AuthorWrapper(Domain.Entities.Author model) : BaseWrapper<Domain.Entities.Author>(model)
{
    [Display(AutoGenerateField = false, Name = "Id")]
    public Guid Id => Model._id;

    [Display(AutoGenerateField = true, Name = "Фамилия")]
    public string? LastName
    {
        get => GetValue<string>();
        set => SetValue(value);
    }
    [Display(AutoGenerateField = true, Name = "Имя")]
    public string? FirstName
    {
        get => GetValue<string>();
        set => SetValue(value);
    }
    [Display(AutoGenerateField = true, Name = "Отчество")]
    public string? SecondName
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = false, Name = "Полное имя")]
    [NotMapped] public string Name => $"{LastName} {FirstName} {SecondName}";

    [Display(AutoGenerateField = true, Name = "Дата рождения")]
    public DateTime? BirthDate
    {
        get => GetValue<DateTime?>();
        set => SetValue(value);
    }
    [Display(AutoGenerateField = true, Name = "Дата смерти")]
    public DateTime? DeathDate
    {
        get => GetValue<DateTime?>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Страна")]
    public RefName? Country
    {
        get => GetValue<RefName?>();
        set => SetValue(value);
    }
}
