using System.Windows.Controls;
using DevExpress.Xpf.Grid;

namespace Personal.WPFClient.Views.Book;

/// <summary>
///     Interaction logic for BookSelectDialogView.xaml
/// </summary>
public partial class BookSelectDialogView : UserControl
{
    public BookSelectDialogView()
    {
        InitializeComponent();
    }

    private void Grid_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
    {
        e.Column.Name = e.Column.FieldName;
    }
}
