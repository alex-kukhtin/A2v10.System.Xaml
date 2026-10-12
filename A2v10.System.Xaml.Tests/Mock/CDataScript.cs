// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.System.Xaml.Tests.Mock;

public class CDataScript
{
    public String? Title { get; set; }

    [WriteAsCData]
    public String? Code { get; set; }
}

[ContentProperty("Code")]
public class CDataContentScript
{
    [WriteAsCData]
    public String? Code { get; set; }
}

public class CDataWrongType
{
    [WriteAsCData]
    public Int32? Value { get; set; }
}
