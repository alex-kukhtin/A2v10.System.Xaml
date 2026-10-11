
namespace A2v10.System.Xaml.Tests.Mock;

public enum DockSide
{
    Left,
    Top,
    Right,
    Bottom
}

// typed attached properties with static setters - the internal mode (UseInternalAttached)
[AttachedProperties("Side,Size")]
[ContentProperty("Children")]
public class Dock() : Container
{
    readonly static Dictionary<Object, DockSide> _sides = [];
    readonly static Dictionary<Object, Int32> _sizes = [];

    public static DockSide? GetSide(Object obj)
    {
        if (_sides.TryGetValue(obj, out DockSide val))
            return val;
        return null;
    }

    public static Int32? GetSize(Object obj)
    {
        if (_sizes.TryGetValue(obj, out Int32 val))
            return val;
        return null;
    }

    public static void SetSide(Object obj, DockSide side)
    {
        _sides[obj] = side;
    }

    public static void SetSize(Object obj, Int32 size)
    {
        _sizes[obj] = size;
    }
}
