using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.System.Xaml.Tests.Mock
{
	public class ResourceDictionary : Dictionary<String, Object>
	{

	}

	[ContentProperty("Children")]
	public class Page : Container
	{

		public ResourceDictionary Resources { get; set; } = new ResourceDictionary();

		public String Title { get; set; }

		public UIElementBase Toolbar { get; set; }
	}
}
