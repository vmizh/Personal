﻿using System.Windows;
using System.Windows.Controls;
using DevExpress.Data;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using Personal.WPFClient.ViewModels;

namespace Personal.WPFClient.Views;

/// <summary>
///     Interaction logic for BooksView.xaml
/// </summary>
public partial class BooksView : UserControl
{
    ComboBoxEditSettings _EditSettings;
    public BooksView()
    {
        InitializeComponent();
    }

    private void Grid_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
    {
        e.Column.Name = e.Column.FieldName;
        switch (e.Column.Name)
        {
            case "Authors":
                var edit = new ComboBoxEditSettings
                {
                    Name = "PART_Editor",
                    ItemsSource = ((BooksWindowViewModel)DataContext).Authors,
                    IsTextEditable = false,
                    DisplayMember = "Name",
                    ValueMember = "Id",
                    AutoComplete = true,
                    StyleSettings = new TokenComboBoxStyleSettings
                    {
                        NewTokenPosition = NewTokenPosition.Far,
                        
                    }
                };
                e.Column.EditSettings = edit;
                break;
        }
    }

    private void DataControlBase_OnAutoGeneratedColumns(object sender, RoutedEventArgs e)
    {
       
    }


    private void GridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
    {
        if (e.Column.FieldName == "Publisher")
        {
            GridControl.TotalSummary.Add(new GridSummaryItem()
            {
                FieldName = "Publisher",
                ShowInColumn = "Publisher",
                SummaryType = SummaryItemType.Custom

            });
        }
    }
}
