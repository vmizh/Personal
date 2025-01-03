using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Personal.Domain.Entities;
using WPFCore;

namespace Personal.WPFClient.Wrappers;

[DebuggerDisplay("Id:{Id} {Name}")]
public class BookPartWrapper(BookPartition model) : BaseWrapper<BookPartition>(model)
{
    [Display(AutoGenerateField = false, Name = "Id")]
    public Guid Id => Model._id;

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
}
