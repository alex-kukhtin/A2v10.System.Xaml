using System.Collections.Generic;

namespace A2v10.System.Xaml.Tests.Mock
{
	[ContentProperty("Activities")]
	public class Sequence : Activtiy
	{
		public List<Activtiy>? Activities { get; set; }
	}
}
