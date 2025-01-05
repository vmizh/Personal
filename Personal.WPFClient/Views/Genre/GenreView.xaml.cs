using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using Personal.WPFClient.ViewModels;

namespace Personal.WPFClient.Views.Genre
{
    /// <summary>
    /// Interaction logic for GenreView.xaml
    /// </summary>
    public partial class GenreView : UserControl
    {
        public GenreView()
        {
            InitializeComponent();
        }

        private void TreeListControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            switch (e.Column.Name)
            {
                case "Partition":
                    e.Column.EditSettings = new ComboBoxEditSettings
                    {
                        ItemsSource = ((GenreWindowViewModel)DataContext).BookPartitions, 
                        IsTextEditable = false,
                        DisplayMember = "Name",
                        AutoComplete = true,
                    
                    };
                    break;
            }
        }

        private void TreeListView_OnDragRecordOver(object sender, DragRecordOverEventArgs e)
        {
            var data = (RecordDragDropData)e.Data.GetData(typeof(RecordDragDropData));  
            if (data != null && data.Records != null && data.Records.Any())  
            {  
                var node = treeListView.GetNodeByContent(data.Records[0]);  

                var targetNode = ((TreeListView)e.OriginalSource).GetNodeByContent(e.TargetRecord);  

                var parentNode = node.ParentNode;  
                
                var parentTargetNode = targetNode?.ParentNode;  

                var parentRowItem = parentNode?.Content;  

                var parentTargetRowItem = parentTargetNode?.Content;  
            }  
        }
    }
}
