using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Personal.Domain.Entities;
using Personal.Domain.Entities.Base;

namespace WPFCore.Wrappers;

[DebuggerDisplay("Id:{Id} {Name}")]
public class GenreWrapper(Genre model) : BaseWrapper<Genre>(model)
{
    [Display(AutoGenerateField = false, Name = "Id")]
    public Guid Id => Model._id;

    [Display(AutoGenerateField = false, Name = "ParentId")]
    public Guid? ParentId
    {
        get => GetValue<Guid?>();
        set => SetValue(value);
    }
    [Display(AutoGenerateField = true, Name = "Наименование")]
    public string Name
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    public string Note
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Раздел")]
    public RefName? Partition
    {
        get => GetValue<RefName?>();
        set => SetValue(value);
    }
}
