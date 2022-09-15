using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.System.Xaml.Tests.Mock
{
	public class Control : UIElementBase
	{
		public Button? Button { get; set; }	
	}

	public class Selector : Control
	{
		public String? Text { get; init; }
	}
}
