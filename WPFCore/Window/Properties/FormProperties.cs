using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using DevExpress.Mvvm.UI;
using WPFCore.Window.Base;

namespace WPFCore.Window.Properties;

public class FormNameProperty
{
    #region Name properties

    public string FormName { set; get; } = "Без названия";
    public Brush FormNameColor { set; get; } = Brushes.Black;
    public FontFamily FormNameFontFamily { set; get; } = new FontFamily("Seqoe UI Light");
    public double FormNameFontSize { set; get; } = 15;

    public FontStyle FormNameFontStyle { set; get; } = FontStyles.Italic;

    public FontWeight FormNameFontWeight { set; get; } = FontWeights.Bold;
    

    #endregion
}

public class FormProperties
{
    public Guid Id { set; get; } 
    public string Name { set; get; }

    public string WindowTitle { set; get; }
    public string Description { set; get; }
    public FormNameProperty FormNameProperty { set; get; } = new FormNameProperty();
    public NotificationProperty NotificationProperty { set; get; } = new NotificationProperty();

    public ObservableCollection<MenuButtonInfo> RightMenuBar { set; get; }
    public ObservableCollection<MenuButtonInfo> LeftMenuBar { set; get; }

    public string? DefaultLayoutString { set; get; } = null;
}

public class NotificationProperty
{
    public string ApplicationId { set; get; } = "Personal";

    public NotificationTemplate PredefinedNotificationTemplate { set; get; }
        = NotificationTemplate.ShortHeaderAndLongText;

    public PredefinedNotificationDuration PredefinedNotificationDuration { set; get; }
        = PredefinedNotificationDuration.Long;

    public NotificationPosition CustomNotificationPosition { set; get; } = NotificationPosition.BottomRight;

}


