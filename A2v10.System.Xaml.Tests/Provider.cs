
namespace A2v10.System.Xaml.Tests;

[TestClass]
[TestCategory("Xaml.Text")]
public class XamlProvider
{
    [TestMethod]
    public void ProviderServices()
    {
        var provider = new XamlServiceProvider();

        var uriContext = provider.GetRequiredXamlService<IUriContext>();
        Assert.IsNotNull(uriContext);
        Assert.IsInstanceOfType<XamlUriContext>(uriContext);

        var rootObjectProvider = provider.GetRequiredXamlService<IRootObjectProvider>();
        Assert.IsNotNull(rootObjectProvider);
        Assert.IsInstanceOfType<XamlRootObjectProvider>(rootObjectProvider);

        var provideValueTarget = provider.GetRequiredXamlService<IProvideValueTarget>();
        Assert.IsNotNull(provideValueTarget);
        Assert.IsInstanceOfType<XamlProvideValueTarget>(provideValueTarget);
    }

    [TestMethod]
    public void UriContext()
    {
        var provider = new XamlServiceProvider();

        var uriContext = provider.GetRequiredXamlService<IUriContext>();
        Assert.IsNotNull(uriContext);
        Assert.IsInstanceOfType<XamlUriContext>(uriContext);

        var uri = new Uri("https://google.com");
        uriContext.BaseUri = uri; 
        Assert.AreEqual(uri, uriContext.BaseUri);
    }
}
