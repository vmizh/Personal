using System;
using System.ComponentModel.DataAnnotations;
using Personal.Domain.Entities;
using Personal.WPFClient.Wrappers.Base;
using WPFCore;

namespace Personal.WPFClient.Wrappers.ReadPagingWrapper;

public class ReadPageWrapper : BaseWrapper<ReadPage>
{
    private ReadPagingWrapper myParent;
    public ReadPageWrapper(ReadPage model, ReadPagingWrapper parent) : base(model)
    {
        myParent = parent;
    }
    
    [Display(AutoGenerateField = true, Name = "Дата")]
    public DateTime Date
    {
        get => GetValue<DateTime>();
        set
        {
            SetValue(value);
            if (myParent is null) return;
            if (myParent.State != StateEnum.New)
                myParent.State = StateEnum.Changed;
            myParent.OnPropertyChanged(nameof(ReadPagingWrapper.DateStart));
        }
    }

    [Display(AutoGenerateField = true, Name = "Кол-во")]
    public int Pages
    {
        get => GetValue<int>();
        set
        {
            SetValue(value);
            if (myParent is null) return;
            if (myParent.State != StateEnum.New)
                myParent.State = StateEnum.Changed;
            myParent.OnPropertyChanged(nameof(ReadPagingWrapper.PageCount));

        }
    }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    public string Note
    {
        get => GetValue<string>();
        set
        {
            SetValue(value);
            if(myParent is not null && myParent.State != StateEnum.New)
                myParent.State = StateEnum.Changed;
        }
    }

    [Display(AutoGenerateField = true, Name = "Хар-ка")]
    public string ExtNote
    {
        get => GetValue<string>();
        set
        {
            SetValue(value);
            if(myParent is not null && myParent.State != StateEnum.New)
                myParent.State = StateEnum.Changed;
        }
    }

    
}
