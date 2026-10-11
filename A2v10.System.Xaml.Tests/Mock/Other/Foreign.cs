
namespace A2v10.System.Xaml.Tests.Mock.Other;

// an element outside the document namespace: the reader does not accept a redefined xmlns
public class Foreign : UIElementBase
{
    public String? Name { get; set; }
}
