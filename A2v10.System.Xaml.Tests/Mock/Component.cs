

namespace A2v10.System.Xaml.Tests.Mock;

[ContentProperty("Slots")]
public class Component : UIElementBase
{
    public SlotDictionary Slots { get; set; } = [];
}

public class SlotDictionary : Dictionary<String, UIElementBase> { };