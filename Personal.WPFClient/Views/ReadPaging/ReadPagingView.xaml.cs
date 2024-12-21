using DevExpress.Utils;
using DevExpress.Xpf.Grid;
using DevExpress.XtraGrid;
using Personal.WPFClient.Helper.Extensions;
using ColumnFilterMode = DevExpress.Xpf.Grid.ColumnFilterMode;

namespace Personal.WPFClient.Views.ReadPaging;

/// <summary>
///     Interaction logic for ReadPagingView.xaml
/// </summary>
public partial class ReadPagingView
{
    public ReadPagingView()
    {
        InitializeComponent();
    }

    private void Grid_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
    {
        e.Column.Name = e.Column.FieldName;
        e.Column.AllowSorting = DefaultBoolean.True;
        if (e.Column.IsTextSortable())
        {
            e.Column.SortMode = ColumnSortMode.DisplayText;
            e.Column.ColumnFilterMode = ColumnFilterMode.DisplayText;
        }
        else
        {
            e.Column.SortMode = ColumnSortMode.Value;
            e.Column.ColumnFilterMode = ColumnFilterMode.Value;
        }
    }
}
