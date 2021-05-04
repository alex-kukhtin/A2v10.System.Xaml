using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.System.Xaml.Tests.Mock
{
	public interface ISupportBinding
	{
		BindImpl BindImpl { get; }
		Bind GetBinding(String name);
		BindCmd GetBindingCommand(String name);
	}
}
