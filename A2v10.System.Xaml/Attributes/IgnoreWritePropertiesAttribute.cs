// Copyright © 2021-2025 Oleksandr Kukhtin. All rights reserved.


using System.Linq;

namespace A2v10.System.Xaml;

[AttributeUsage(AttributeTargets.Class)]
public class IgnoreWritePropertiesAttribute(String list) : Attribute
{
    public HashSet<String> Attrs { get; } = list
        .Split(",", StringSplitOptions.RemoveEmptyEntries)
        .Select(x => x.Trim())
        .ToHashSet<String>();

    public Boolean Contains(String attr)
    {
        return Attrs.Contains(attr);
    }
}
