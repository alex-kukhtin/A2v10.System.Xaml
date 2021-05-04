using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace A2v10.System.Xaml.Tests.Mock
{

	public class BindImpl
	{
		IDictionary<String, BindBase> _bindings;

		public BindBase SetBinding(String name, BindBase bind)
		{
			if (_bindings == null)
				_bindings = new Dictionary<String, BindBase>();
			_bindings.Add(name, bind);
			return bind;
		}

		public void RemoveBinding(String name)
		{
			if (_bindings == null) return;
			if (_bindings.ContainsKey(name))
				_bindings.Remove(name);
		}

		public Bind GetBinding(String name)
		{
			if (_bindings == null)
				return null;
			if (_bindings.TryGetValue(name, out BindBase bind))
			{
				if (bind is Bind)
					return bind as Bind;
				throw new InvalidOperationException($"Binding '{name}' must be a Bind");
			}
			return null;
		}

		public BindCmd GetBindingCommand(String name)
		{
			if (_bindings == null)
				return null;
			if (_bindings.TryGetValue(name, out BindBase bind))
			{
				if (bind is BindCmd)
					return bind as BindCmd;
				throw new InvalidOperationException($"Binding '{name}' must be a BindCmd");
			}
			return null;
		}
	}


	public abstract class BindBase : MarkupExtension, ISupportBinding
	{
		BindImpl _bindImpl;

		public BindImpl BindImpl
		{
			get
			{
				if (_bindImpl == null)
					_bindImpl = new BindImpl();
				return _bindImpl;
			}
		}

		public override Object ProvideValue(IServiceProvider serviceProvider)
		{
			if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget iTarget)
				return null;
			var targetProp = iTarget.TargetProperty as PropertyInfo;
			var targetObj = iTarget.TargetObject as ISupportBinding;
			if ((targetObj == null) && (targetProp == null))
				return null;
			targetObj.BindImpl.SetBinding(targetProp.Name, this);
			if (targetProp.PropertyType.IsValueType)
				return Activator.CreateInstance(targetProp.PropertyType);
			return null; // is object
		}

		public Bind GetBinding(String name)
		{
			return _bindImpl?.GetBinding(name);
		}

		/*
		void SetBinding(String name, BindBase bind)
		{
			_bindImpl.SetBinding(name, bind);
		}
		*/

		public BindCmd GetBindingCommand(String name)
		{
			return _bindImpl?.GetBindingCommand(name);
		}
	}

	public class Bind : BindBase, ISupportInitialize
	{
		public String Path { get; set; }
		public String Format { get; set; }
		public Boolean HideZeros { get; set; }
		public String Mask { get; set; }
		public Boolean NegativeRed { get; set; }

		public Bind()
		{

		}
		public Bind(String path)
		{
			Path = path;
		}


		// for text bindings only


		#region ISupportInitialize
		public void BeginInit()
		{
		}

		public void EndInit()
		{
		}
		#endregion
	}
}
