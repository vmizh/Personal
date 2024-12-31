using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Mvvm;
using Personal.Domain.Entities;
using Personal.WPFClient.Helper.Window;
using Personal.WPFClient.Repositories.Base;
using Personal.WPFClient.Repositories.Layout;
using WPFCore.Window.Properties;

namespace WPFCore.ViewModel;

[SuppressMessage("ReSharper", "AsyncVoidLambda")]
public class ViewModelDialogBase : ViewModelBase
{
    #region Services

    [Display(AutoGenerateField = false)]
    public ILayoutSerializationService LayoutSerializationService => GetService<ILayoutSerializationService>();

    #endregion

    #region Constructors

    public ViewModelDialogBase()
    {
        OnWindowClosingCommand = new DelegateCommand(
            async () => await OnWindowClosingAsync(),
            () => true);
        OnWindowLoadedCommand = new DelegateCommand(
            async () => await OnWindowLoadedAsync(),
            () => true);

        OnWindowResetLayoutCommand = new DelegateCommand(
            async () => await OnWindowResetLayoutAsync(),
            () => true);
    }

    #endregion


    #region Properties

    public MessageResult DialogResult = MessageResult.No;

    public UserControl CustomDataUserControl { set; get; }

    #endregion

    #region Methods

    #endregion

    #region Fields

    public FormProperties Properties { get; } = new FormProperties();

    public UserControl DataControl { get; protected set; }
    protected ILayoutRepository myLayoutRepository;

    #endregion

    #region Commands

    [Display(AutoGenerateField = false)] public ICommand OnWindowClosingCommand { get; }

    [Display(AutoGenerateField = false)] public ICommand OnWindowResetLayoutCommand { get; }

    protected virtual async Task OnWindowClosingAsync()
    {
        try
        {
            if (LayoutSerializationService is not null && myLayoutRepository is not null)
            {
                var l = new Layout
                {
                    _id = Properties.Id,
                    LayoutString = LayoutSerializationService.Serialize()
                };
                await ((BaseRepository<Layout>)myLayoutRepository).UpdateAsync(l);
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }


    [Display(AutoGenerateField = false)] public ICommand OnWindowLoadedCommand { get; }

    protected virtual async Task OnWindowLoadedAsync()
    {
        try
        {
            if (LayoutSerializationService is not null && myLayoutRepository is not null)
            {
                Properties.DefaultLayoutString = LayoutSerializationService.Serialize();
                var l = await ((BaseRepository<Layout>)myLayoutRepository).GetByIdAsync(Properties.Id);
                if (l is null) return;
                LayoutSerializationService.Deserialize(l.LayoutString);
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    private async Task OnWindowResetLayoutAsync()
    {
        LayoutSerializationService.Deserialize(Properties.DefaultLayoutString);
    }

    #endregion
}
