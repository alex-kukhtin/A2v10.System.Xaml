
namespace A2v10.System.Xaml.Tests.Mock;

public class ResourceDictionary : Dictionary<String, Object>
{

}

[ContentProperty("Children")]
public class Page : Container
{
        public String? Code { get; init; }

        public ResourceDictionary Resources { get; set; } = [];

	public String? Title { get; set; }

	public UIElementBase? Toolbar { get; set; }
}
