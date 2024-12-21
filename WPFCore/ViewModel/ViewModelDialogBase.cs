using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using DevExpress.Mvvm;
using WPFCore.Window.Properties;

namespace WPFCore.ViewModel;

[SuppressMessage("ReSharper", "AsyncVoidLambda")]
public class ViewModelDialogBase : ViewModelBase
{
    #region Constructors

    public ViewModelDialogBase()
    {
       
    }
    
    #endregion


    #region Properties

    public new MessageResult DialogResult = MessageResult.No;

    public UserControl CustomDataUserControl { set; get; }

    #endregion

    #region Methods

    

    #endregion

    #region Fields

    public FormProperties Properties { get; private set; }

    public UserControl DataControl { get; protected set; }

   
    #endregion

    #region Commands

    
    #endregion

   

    
}
