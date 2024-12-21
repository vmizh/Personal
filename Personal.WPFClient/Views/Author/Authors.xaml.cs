using System.Windows.Controls;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using Personal.WPFClient.ViewModels;
using Personal.WPFClient.Views.Base;

namespace Personal.WPFClient.Views;

/// <summary>
///     Interaction logic for Authors.xaml
/// </summary>
public partial class Authors : IUpdateVisualObject
{
    public Authors()
    {
        InitializeComponent();
    }

    private void Grid_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
    {
        e.Column.Name = e.Column.FieldName;
        switch (e.Column.Name)
        {
            case "Country":
                e.Column.EditSettings = new ComboBoxEditSettings
                {
                    ItemsSource = ((AuthorsWindowViewModel)DataContext).Countries, 
                    IsTextEditable = false,
                    DisplayMember = "Name",
                    AutoComplete = true,
                    
                };
                break;
        }
    }


    public void CloseEditors()
    {
        tableView.CloseEditor();
        Dispatcher.Invoke(() => tableView.Focus());
    }
}
