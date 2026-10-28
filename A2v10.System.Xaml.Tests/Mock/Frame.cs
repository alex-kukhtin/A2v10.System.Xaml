// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.System.Xaml.Tests.Mock;

// a typed content property that holds one object
[ContentProperty("Body")]
public class Frame : UIElementBase
{
    public UIElementBase? Body { get; set; }
}
