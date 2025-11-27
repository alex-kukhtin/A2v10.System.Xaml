
using Microsoft.Testing.Platform.Services;

namespace A2v10.System.Xaml.Tests.Mock;

[AttachedProperties("Col,Row")]
[ContentProperty("Children")]
public class Grid(IServiceProvider serviceProvider) : Container
{
    private readonly IAttachedPropertyManager _attachedPropertyManager = serviceProvider.GetRequiredService<IAttachedPropertyManager>();

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
