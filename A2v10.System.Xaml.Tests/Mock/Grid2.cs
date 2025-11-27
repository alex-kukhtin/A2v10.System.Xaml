

namespace A2v10.System.Xaml.Tests.Mock;

[AttachedProperties("Col,Row")]
[ContentProperty("Children2")]
public class Grid2() : Container
{
    #region Attached Properties

    readonly Dictionary<Object, String> _rows = [];
    readonly Dictionary<Object, String> _cols = [];
    public String? GetCol(Object obj)
    {
        if (_cols.TryGetValue(obj, out String? val))
            return val;
        return null;
    }

    public String? GetRow(Object obj)
    {
        if (_rows.TryGetValue(obj, out String? val))
            return val;
        return null;
    }

    public void SetRow(Object obj, String row)
    {
        _rows[obj] = row;
    }
    public void SetCol(Object obj, String col)
    {
        _cols[obj] = col;
    }
    #endregion
}
