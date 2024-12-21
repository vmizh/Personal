using System.Windows.Controls;
using DevExpress.Xpf.Grid;

namespace Personal.WPFClient.Views.Author;

/// <summary>
///     Interaction logic for AuthorSelectDialogView.xaml
/// </summary>
public partial class AuthorSelectDialogView : UserControl
{
    public AuthorSelectDialogView()
    {
        InitializeComponent();
    }

    private void Grid_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
    {
        e.Column.Name = e.Column.FieldName;
    }

}


