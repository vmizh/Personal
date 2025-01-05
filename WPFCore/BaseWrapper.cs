using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Personal.WPFClient.Wrappers;
using WPFCore.ViewModel;

namespace WPFCore;

public class BaseWrapper<T>(T model) : NotifyDataErrorInfoBase
{
    [Display(AutoGenerateField = false)] public T Model { get; } = model;

    [Display(AutoGenerateField = false)] public StateEnum State { get; set; } = StateEnum.NotDefinition;

    protected virtual void SetValue<TValue>(TValue value,
        [CallerMemberName] string propertyName = null)
    {
        typeof(T).GetProperty(propertyName)?.SetValue(Model, value);
        if(State != StateEnum.New)
            State = StateEnum.Changed;
        OnPropertyChanged(propertyName);
        ValidatePropertyInternal(propertyName, value);
    }

    protected virtual TValue GetValue<TValue>([CallerMemberName] string propertyName = null)
    {
        return (TValue)typeof(T).GetProperty(propertyName)?.GetValue(Model);
    }

    private void ValidatePropertyInternal(string propertyName, object currentValue)
    {
        ClearErrors(propertyName);

        ValidateDataAnnotations(propertyName, currentValue);

        ValidateCustomErrors(propertyName);
    }

    private void ValidateDataAnnotations(string propertyName, object currentValue)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(Model) { MemberName = propertyName };
        Validator.TryValidateProperty(currentValue, context, results);

        foreach (var result in results) AddError(propertyName, result.ErrorMessage);
    }

    private void ValidateCustomErrors(string propertyName)
    {
        var errors = ValidateProperty(propertyName);
        if (errors == null) return;
        foreach (var error in errors)
            AddError(propertyName, error);
    }

    protected virtual IEnumerable<string> ValidateProperty(string propertyName)
    {
        return null;
    }
}
