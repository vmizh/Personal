using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPFCore.ViewModel;

public class ViewModelBase : DevExpress.Mvvm.ViewModelBase, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
