using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Personal.Domain.Entities;
using Personal.Domain.Entities.Base;
using WPFCore;

namespace Personal.WPFClient.Wrappers;

[DebuggerDisplay("Id:{Id} {Name}")]
public class BookWrapper : BaseWrapper<Book>
{
    private readonly object myAuthors;

    public BookWrapper(Book model) : base(model)
    {
        if (model.AuthorList is not null && model.AuthorList.Count > 0)
        {
            myAuthors = new List<object>();
            foreach (var id in model.AuthorList.Select(_ => _.Id)) ((List<object>)myAuthors).Add(id);
        }
    }

    public List<RefName> AuthorAllList { set; get; }

    [Display(AutoGenerateField = false, Name = "Id")]
    public Guid Id => Model._id;

    [Display(AutoGenerateField = true, Name = "Наименование")]
    public string Name
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Издательство")]
    public string? Publisher
    {
        get => GetValue<string?>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Год.изд.")]
    public int? PublisherYear
    {
        get => GetValue<int?>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Аннотация")]
    public string? Annotation
    {
        get => GetValue<string?>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Авторы")]
    public string Authors
    {
        get
        {
            var s = new StringBuilder();
            if (AuthorList is null || AuthorList.Count <= 0) return s.ToString();
            foreach (var auth in AuthorList) s.Append($"{auth}; ");
            return s.ToString();
        }
    }

    [Display(AutoGenerateField = true, Name = "Авторы")]
    [CanBeNull]
    public List<RefName> AuthorList
    {
        get => GetValue<List<RefName>?>();
        set => SetValue(value);
    }
}
