using System.Collections.Generic;

namespace Personal.WPFClient.Helper;

public static class Windows
{
    public static List<System.Windows.Window> OpenedWindow { set; get; }

    static Windows()
    {
        OpenedWindow = new List<System.Windows.Window>();
    }
}
