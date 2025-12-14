
namespace A2v10.System.Xaml.Tests.Mock;


[ContentProperty("Children")]
public class Dialog : Container
{
    public UIElementCollection Buttons { get; set; } = [];

}
