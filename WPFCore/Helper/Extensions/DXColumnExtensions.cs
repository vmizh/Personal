using System;
using System.Windows.Media.Animation;
using DevExpress.Xpf.Grid;

namespace Personal.WPFClient.Helper.Extensions;

public static class DXColumnExtensions
{
    public static bool IsTextSortable(this ColumnBase col)
    {
        return col.FieldType != typeof(string) && col.FieldType != typeof(int?)
                                               && col.FieldType != typeof(int)
                                               && col.FieldType != typeof(DateTime)
                                               && col.FieldType != typeof(DateTime?)
                                               && col.FieldType != typeof(float)
                                               && col.FieldType != typeof(float?)
                                               && col.FieldType != typeof(double)
                                               && col.FieldType != typeof(double?)
                                               && col.FieldType != typeof(decimal)
                                               && col.FieldType != typeof(decimal?);
    }
}
