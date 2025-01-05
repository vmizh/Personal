using System.Windows.Controls;
using DevExpress.Xpf.Grid;

namespace Personal.WPFClient.Views.Genre;

/// <summary>
///     Interaction logic for GenreSelectDialogView.xaml
/// </summary>
public partial class GenreSelectDialogView : UserControl
{
    public GenreSelectDialogView()
    {
        InitializeComponent();
    }

    private void TreeListControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
    {
        e.Column.Name = e.Column.FieldName;
        e.Column.ReadOnly = true;
    }

    private void Grid_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
    {
        e.Column.Name = e.Column.FieldName;
    }
}
