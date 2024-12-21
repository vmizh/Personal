using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using Personal.Domain.Entities;
using Personal.Domain.Entities.Base;
using WPFCore;

namespace Personal.WPFClient.Wrappers.ReadPagingWrapper;

[DebuggerDisplay("Id:{Id} {Name}")]
public class ReadPagingWrapper : BaseWrapper<ReadPaging>
{
    public ReadPagingWrapper(ReadPaging model) : base(model)
    {
        if (model.Read.Any())
            foreach (var p in model.Read)
                Read.Add(new ReadPageWrapper(p, this));
        Read.CollectionChanged += Read_CollectionChanged;
    }

    [Display(AutoGenerateField = false, Name = "Id")]
    public Guid Id => Model._id;

    [Display(AutoGenerateField = true, Name = "Авторы")]
    [ReadOnly(true)]
    public string Name
    {
        get => GetValue<string>();
        set => SetValue(value);
    }


    [Display(AutoGenerateField = true, Name = "Закончено")]
    public DateTime? Readed
    {
        get => GetValue<DateTime?>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Примечания")]
    public string? Note
    {
        get => GetValue<string?>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Книга")]
    public RefName Book
    {
        get => GetValue<RefName>();
        set => SetValue(value);
    }

    public ObservableCollection<ReadPageWrapper> Read { set; get; } = [];

    [Display(AutoGenerateField = true, Name = "Прочитано (стр)")]
    public int PageCount => Read.Sum(_ => _.Pages);

    [Display(AutoGenerateField = true, Name = "Начато")]
    public DateTime? DateStart => Read.Any() ? Read.Min(_ => _.Date) : null;

    private void Read_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewItems.Cast<ReadPageWrapper>()) Model.Read.Add(item.Model);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems.Cast<ReadPageWrapper>()) Model.Read.Remove(item.Model);
                break;
        }
    }
}
