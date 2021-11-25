
namespace A2v10.System.Xaml.Tests.Mock;
public interface ISupportBinding
{
	BindImpl BindImpl { get; }
	Bind? GetBinding(String name);
	BindCmd? GetBindingCommand(String name);
}

