using System.Windows;
using System.Windows.Media;

namespace Personal.WPFClient.Helper.Window;

public interface IWindowManager
{
    MessageBoxResult ShowMessageBox(string messageBoxText, string caption, MessageBoxButton button);
    void ShowMessageBox(string messageBoxText, string caption);

    MessageBoxResult ShowMessageBox(string messageBoxText, string caption, MessageBoxButton button,
        MessageBoxImage image);

    //WindowManager.KursDialogResult ShowKursDialog(string text, string titleText, Brush titleTextColor,
    //    WindowManager.KursDialogResult result);

    //WindowManager.KursDialogResult ShowKursDialog(string text, string titleText);

    MessageBoxResult ShowWinUIMessageBox(string messageBoxText, string caption,
        MessageBoxButton button, MessageBoxImage image, MessageBoxResult result, MessageBoxOptions options);

    MessageBoxResult ShowWinUIMessageBox(string messageBoxText, string caption,
        MessageBoxButton button, MessageBoxImage image);

    MessageBoxResult ShowWinUIMessageBox(System.Windows.Window win, string messageBoxText, string caption,
        MessageBoxButton button, MessageBoxImage image);

    MessageBoxResult ShowWinUIMessageBox(string messageBoxText, string caption, MessageBoxButton yesNo);
    MessageBoxResult ShowWinUIMessageBox(string messageBoxText, string caption);
}
