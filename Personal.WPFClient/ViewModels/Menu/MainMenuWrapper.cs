using System;
using System.Diagnostics;
using System.Windows.Input;
using DevExpress.Mvvm;
using Newtonsoft.Json;
using Personal.Domain.Redis;
using ServiceStack.Redis;
using ViewModelBase = WPFCore.ViewModel.ViewModelBase;

namespace Personal.WPFClient.Wrappers.Menu;

[DebuggerDisplay("Id:{Id} {Name}")]
public class MainMenuWrapper : ViewModelBase
{
    private readonly RedisManagerPool redisManager;

    public MainMenuWrapper(RedisManagerPool redisManager)
    {
        this.redisManager = redisManager;
        ClickCommand = new DelegateCommand(OnClickExecute);
    }

    public StateEnum State { get; set; }

    public ICommand ClickCommand { get; }

    public Guid Id { set; get; }

    public string Name
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    public string Title
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    public byte[] Picture
    {
        get => GetValue<byte[]>();
        set => SetValue(value);
    }

    private void OnClickExecute()
    {
        var redisClient = redisManager.GetClient();
        var message = new RedisMessage
        {
            MessageType = RedisMessageDocumentOperationTypeEnum.Open,
            DocDate = DateTime.Now,
            Message =
                $"Вызов меню '{Id}' {Name}"
        };
        message.ExternalValues.Add("MenuId", Id);
        var jsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };
        redisClient.PublishMessage("Menu",
            JsonConvert.SerializeObject(message, jsonSerializerSettings));
    }
}
