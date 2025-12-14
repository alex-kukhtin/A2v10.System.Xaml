// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.System.Xaml.Tests.Mock;

public class Setter
{
    public String? Property { get; set; }
    public Object? Value { get; set; }
}


public class Style : List<Setter>
{
}

[WrapContent]   
public class Styles : Dictionary<String, Style>
{
}

