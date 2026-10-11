
namespace A2v10.System.Xaml.Tests.Mock;

// a value type with no IXamlConverter: the writer has no text form for it
public struct Offset
{
    public Int32 X { get; set; }
}

public class Shape
{
    public Offset Origin { get; set; }
}
