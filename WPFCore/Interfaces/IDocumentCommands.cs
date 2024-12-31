using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;

namespace WPFCore.Interfaces;

public interface IDocumentCommands
{
    ICommand DocumentOpenCommand { set; get; }
    bool CanDocumentOpen { set; get; }
    Task DocumentOpenAsync();

    ICommand DocumentCloseCommand { set; get; }
    bool CanDocumentClose { set; get; }
    Task DocumentCloseAsync();

    AsyncCommand DocumentSaveCommand { set; get; }
    bool CanDocumentSave { set; get; }
    Task DocumentSaveAsync();

    AsyncCommand DocumentRefreshCommand { set; get; }
    bool CanDocumentRefresh { set; get; }
    Task DocumentRefreshAsync();

    ICommand DocumentDeleteCommand { set; get; }
    bool CanDocumentDelete { set; get; }
    Task DocumentDeleteAsync();

    ICommand DocumentNewEmptyCommand { set; get; }
    bool CanDocumentNewEmpty { set; get; }
    Task DocumentNewEmptyAsync();

    ICommand DocumentNewRequisiteCommand { set; get; }
    bool CanDocumentNewRequisite { set; get; }
    Task DocumentNewRequisiteAsync();

    ICommand DocumentNewCopyCommand { set; get; }
    bool CanDocumentNewCopy { set; get; }
    Task DocumentNewCopyAsync();
}
