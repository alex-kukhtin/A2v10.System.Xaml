// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using System.Linq;

using Microsoft.Testing.Platform.Services;

namespace A2v10.System.Xaml.Tests.Mock;

public record RowDefinition
{
    public Length? Height { get; set; }
}

[WrapContent]
public class RowDefinitions : List<RowDefinition>, IXamlConverter
{
    public string? ToXamlString()
    {
        return String.Join(", ", this.Select(x => x.Height is IXamlConverter xamlConv ? xamlConv.ToXamlString() : x.Height?.ToString()));
    }
}

public enum Overflow
{
    Visible,
    Hidden,
    Auto,
    True = Visible,
    False = Hidden,
}

[AttachedProperties("Col,Row")]
[ContentProperty("Children")]
public class Grid(IServiceProvider serviceProvider) : Container, ISupportAttached
{
    private readonly IAttachedPropertyManager _attachedPropertyManager = serviceProvider.GetRequiredService<IAttachedPropertyManager>();


    public RowDefinitions? Rows { get; set; }

    public Overflow? Overflow { get; set; }

    #region Attached Properties

    // for testing attached properties  
    public IAttachedPropertyManager AttachedPropertyManager => _attachedPropertyManager;

    public Int32? GetCol(Object obj)
    {
        return _attachedPropertyManager.GetProperty<Int32?>("Grid.Col", obj);
    }

    public Int32? GetRow(Object obj)
    {
        return _attachedPropertyManager.GetProperty<Int32?>("Grid.Row", obj);
    }

    public void SetRow(Object obj, Int32 row)
    {
        _attachedPropertyManager.SetProperty("Grid.Row", obj, row);
    }
    public void SetCol(Object obj, Int32 col)
    {
        _attachedPropertyManager.SetProperty("Grid.Col", obj, col);
    }
    #endregion
}
