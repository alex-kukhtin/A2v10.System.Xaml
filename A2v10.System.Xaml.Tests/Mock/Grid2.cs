

namespace A2v10.System.Xaml.Tests.Mock;

[AttachedProperties("Col,Row")]
[ContentProperty("Children")]
public class Grid2() : Container
{
    #region Attached Properties

    readonly static Dictionary<Object, String> _rows = [];
    readonly static Dictionary<Object, String> _cols = [];
    public static String? GetCol(Object obj)
    {
        if (_cols.TryGetValue(obj, out String? val))
            return val;
        return null;
    }

    public static String? GetRow(Object obj)
    {
        if (_rows.TryGetValue(obj, out String? val))
            return val;
        return null;
    }

    public static void SetRow(Object obj, String row)
    {
        _rows[obj] = row;
    }
    public static void SetCol(Object obj, String col)
    {
        _cols[obj] = col;
    }
    #endregion
}
