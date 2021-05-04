using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.System.Xaml.Tests.Mock
{
	public class UIElementBase : ISupportBinding
	{
		public Boolean? Show { get; set; }
		public Boolean? If { get; set; }
		public Thickness Padding { get; set; }

		BindImpl _bindImpl;

		#region ISupportBinding
		public BindImpl BindImpl
		{
			get
			{
				if (_bindImpl == null)
					_bindImpl = new BindImpl();
				return _bindImpl;
			}
		}

		public Bind GetBinding(String name)
		{
			return _bindImpl?.GetBinding(name);
		}

		public BindCmd GetBindingCommand(String name)
		{
			return _bindImpl?.GetBindingCommand(name);
		}
		#endregion
	}

	[TypeConverter(typeof(UICollectionConverter))]
	public class UIElementCollection : List<UIElementBase>
	{
		public UIElementCollection()
		{

		}
	}

	public class UICollectionConverter : TypeConverter
	{
		public override Boolean CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(String))
				return true;
			else if (sourceType == typeof(UIElementBase))
				return true;
			return base.CanConvertFrom(context, sourceType);
		}

		public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
		{
			if (value == null)
				return null;
			if (value is String)
			{
				var x = new UIElementCollection
				{
					new Span() { Content = value }
				};
				return x;
			}
			else if (value is UIElementBase)
			{
				var x = new UIElementCollection
				{
					value as UIElementBase
				};
				return x;
			}
			return base.ConvertFrom(context, culture, value);
		}
	}
}
