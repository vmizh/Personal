using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using Personal.Domain.Entities;
using Personal.WPFClient.Helper.Window;
using Personal.WPFClient.Repositories.Base;
using Personal.WPFClient.Repositories.Layout;
using WPFCore.Interfaces;
using WPFCore.Window.Base;
using WPFCore.Window.Properties;
using UserControl = System.Windows.Controls.UserControl;
using Win = Personal.WPFClient.Helper.Windows;
using WindowStartupLocation = Personal.Domain.Helper.WindowStartupLocation;
using WindowState = Personal.Domain.Helper.WindowState;

namespace WPFCore.ViewModel;

[SuppressMessage("ReSharper", "AsyncVoidLambda")]
public class ViewModelWindowBase : ViewModelBase, IDocumentCommands
{
    #region Constructors

    public ViewModelWindowBase()
    {
        Properties = new FormProperties();

        OnWindowClosingCommand = new DelegateCommand(
            async () => await OnWindowClosingAsync(),
            () => true);
        OnWindowLoadedCommand = new DelegateCommand(
            async () => await OnWindowLoadedAsync(),
            () => true);

        OnWindowResetLayoutCommand = new DelegateCommand(
            async () => await OnWindowResetLayoutAsync(),
            () => true);

        DocumentOpenCommand = new DelegateCommand(
            async () => await DocumentOpenAsync(),
            () => CanDocumentOpen);
        DocumentCloseCommand = new DelegateCommand(
            async () => await DocumentCloseAsync(),
            () => CanDocumentClose);
        DocumentDeleteCommand = new DelegateCommand(
            async () => await DocumentDeleteAsync(),
            () => CanDocumentDelete);
        DocumentSaveCommand = new AsyncCommand(
            async () => await DocumentSaveAsync(),
            () => CanDocumentSave);
        DocumentRefreshCommand = new AsyncCommand(
            async () => await DocumentRefreshAsync(),
            () => CanDocumentRefresh);
        DocumentNewEmptyCommand = new DelegateCommand(
            async () => await DocumentNewEmptyAsync(),
            () => CanDocumentNewEmpty);
        DocumentNewCopyCommand = new DelegateCommand(
            async () => await DocumentNewCopyAsync(),
            () => CanDocumentNewCopy);
        DocumentNewRequisiteCommand = new DelegateCommand(
            async () => await DocumentNewRequisiteAsync(),
            () => CanDocumentNewRequisite);
    }

    #endregion

    #region Methods

    public virtual async Task Show()
    {
        FormWindow = new FormWindowBase
        {
            DataContext = this
        };

        FormWindow.Show();
    }

    #endregion

    #region Fields

    protected ILayoutRepository myLayoutRepository;

    public FormProperties Properties { get; }

    public UserControl DataControl { get; protected set; }

    protected FormWindowBase FormWindow { get; set; }

    #endregion

    #region Commands

    [Display(AutoGenerateField = false)] public ICommand OnWindowClosingCommand { get; private set; }

    [Display(AutoGenerateField = false)] public ICommand OnWindowResetLayoutCommand { get; private set; }

    protected virtual async Task OnWindowClosingAsync()
    {
        Win.OpenedWindow.Remove(FormWindow);
        try
        {
            if (LayoutSerializationService is not null && myLayoutRepository is not null)
            {
                var l = new Layout
                {
                    _id = Properties.Id,
                    FormHeight = FormWindow.Height,
                    FormWidth = FormWindow.Width,
                    FormLeft = FormWindow.Left,
                    FormTop = FormWindow.Top,
                    FormStartLocation = GetLocation(FormWindow.WindowStartupLocation),
                    FormState = GetWinState(FormWindow.WindowState),
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

    private WindowState GetWinState(System.Windows.WindowState state)
    {
        return state switch
        {
            System.Windows.WindowState.Maximized => WindowState.Maximized,
            System.Windows.WindowState.Normal => WindowState.Normal,
            System.Windows.WindowState.Minimized => WindowState.Minimized,
            _ => WindowState.Normal
        };
    }

    private WindowStartupLocation GetLocation(System.Windows.WindowStartupLocation loc)
    {
        return loc switch
        {
            System.Windows.WindowStartupLocation.Manual => WindowStartupLocation.Manual,
            System.Windows.WindowStartupLocation.CenterOwner => WindowStartupLocation.CenterOwner,
            System.Windows.WindowStartupLocation.CenterScreen => WindowStartupLocation.CenterScreen,
            _ => WindowStartupLocation.Manual
        };
    }

    [Display(AutoGenerateField = false)] public ICommand OnWindowLoadedCommand { get; private set; }

    protected virtual async Task OnWindowLoadedAsync()
    {
        Win.OpenedWindow.Add(FormWindow);
        try
        {
            if (LayoutSerializationService is not null && myLayoutRepository is not null)
            {
                Properties.DefaultLayoutString = LayoutSerializationService.Serialize();
                var l = await ((BaseRepository<Layout>)myLayoutRepository).GetByIdAsync(Properties.Id);
                if (l is null) return;

                FormWindow.Height = l.FormHeight;
                FormWindow.Width = l.FormWidth;
                FormWindow.Left = l.FormLeft;
                FormWindow.Top = l.FormTop;
                FormWindow.WindowStartupLocation = l.FormStartLocation switch
                {
                    WindowStartupLocation.CenterOwner => System.Windows.WindowStartupLocation.CenterOwner,
                    WindowStartupLocation.CenterScreen => System.Windows.WindowStartupLocation.CenterScreen,
                    WindowStartupLocation.Manual => System.Windows.WindowStartupLocation.Manual,
                    _ => FormWindow.WindowStartupLocation
                };
                FormWindow.WindowState = l.FormState switch
                {
                    WindowState.Minimized => System.Windows.WindowState.Minimized,
                    WindowState.Maximized => System.Windows.WindowState.Maximized,
                    WindowState.Normal => System.Windows.WindowState.Normal,
                    _ => FormWindow.WindowState
                };
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

    public ICommand DocumentOpenCommand { get; set; }
    public virtual bool CanDocumentOpen { get; set; }

    public virtual async Task DocumentOpenAsync()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    public ICommand DocumentCloseCommand { get; set; }
    public virtual bool CanDocumentClose { get; set; } = true;

    public virtual async Task DocumentCloseAsync()
    {
        FormWindow?.Close();
    }

    public AsyncCommand DocumentSaveCommand { get; set; }
    public virtual bool CanDocumentSave { get; set; }

    public virtual async Task DocumentSaveAsync()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    public AsyncCommand DocumentRefreshCommand { get; set; }
    public virtual bool CanDocumentRefresh { get; set; } = true;

    public virtual async Task DocumentRefreshAsync()
    {
    }

    public ICommand DocumentDeleteCommand { get; set; }
    public virtual bool CanDocumentDelete { get; set; }

    public virtual async Task DocumentDeleteAsync()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    public ICommand DocumentNewEmptyCommand { get; set; }
    public virtual bool CanDocumentNewEmpty { get; set; }

    public virtual async Task DocumentNewEmptyAsync()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    public ICommand DocumentNewRequisiteCommand { get; set; }
    public virtual bool CanDocumentNewRequisite { get; set; }

    public virtual async Task DocumentNewRequisiteAsync()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    public ICommand DocumentNewCopyCommand { get; set; }
    public virtual bool CanDocumentNewCopy { get; set; }

    public virtual async Task DocumentNewCopyAsync()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    #endregion

    #region Services

    [Display(AutoGenerateField = false)]
    public IDispatcherService DispatcherService => GetService<IDispatcherService>();

    [Display(AutoGenerateField = false)] public IDialogService DialogService => GetService<IDialogService>();

    [Display(AutoGenerateField = false)]
    public ILayoutSerializationService LayoutSerializationService => GetService<ILayoutSerializationService>();

    [Display(AutoGenerateField = false)]
    public INotificationService KursNotyficationService => GetService<INotificationService>();

    #endregion

    #region Properties


    #endregion
}
