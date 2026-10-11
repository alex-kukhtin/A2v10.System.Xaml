
namespace A2v10.System.Xaml.Tests.Mock;

// dictionaries whose key is not a String: x:Key is text, converted to the key type
public class NumberedItems : Dictionary<UInt32, Span> { }
public class SideItems : Dictionary<DockSide, Span> { }
public class RatedItems : Dictionary<Double, Span> { }

public class Keyed : UIElementBase
{
    public NumberedItems Numbered { get; set; } = [];
    public SideItems Sides { get; set; } = [];
}
