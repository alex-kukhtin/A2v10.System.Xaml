// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace A2v10.System.Xaml;

public record NamespaceDefinition
{
	public NamespaceDefinition(String ns, Assembly ass)
    {
		_namespace = ns;
		_assembly = ass;
    }

	public static NamespaceDefinition SpecialNamespace()
    {
		return new NamespaceDefinition()
		{
		};
    }
	private NamespaceDefinition()
    {
		IsSpecial = true;
    }

	private readonly Assembly? _assembly;
	private readonly String? _namespace;

	public String Namespace => !IsSpecial
		? _namespace ?? throw new XamlException("Namespace not defined")
		: throw new XamlException("Invalid special namespace (Namespace)");
	public Assembly Assembly => !IsSpecial
		? _assembly ?? throw new XamlException("Assembly not defined ")
		: throw new XamlException("Invalid special namespace (Assembly)");
	public Boolean IsCamelCase { get; init; }
	public Boolean IsSpecial { get; }
	public Boolean IsSkip { get; set; }
}

public record ClassNamePair(String Prefix, String Namespace, String ClassName, Boolean IsCamelCase);

public class NodeBuilder
{

	private readonly Dictionary<String, NamespaceDefinition> _namespaces = new();
	private readonly XamlServicesOptions? _options;
	private readonly XamlServiceProvider _serviceProvider;
	private readonly TypeDescriptorCache _typeCache;

	private readonly Lazy<List<Action>> _deferExec = new();

	public NodeBuilder(XamlServiceProvider serviceProvider, TypeDescriptorCache typeCache, XamlServicesOptions? options)
	{
		_serviceProvider = serviceProvider;
		_typeCache = typeCache;
		_options = options;
	}

	NamespaceDef? IsCustomNamespace(String value)
	{
		if (_options == null || _options.Namespaces == null)
			return null;
		value = value.ToLowerInvariant();
		return _options.Namespaces.FirstOrDefault(x => x.Name == value);
	}

	public Boolean EnableMarkupExtensions => _options == null || !_options.DisableMarkupExtensions;

	public IAttachedPropertyManager AttachedPropertyManager => _serviceProvider.GetService<IAttachedPropertyManager>();

	private static readonly Regex _namespaceRegEx = new(@"^\s*clr-namespace\s*:\s*([\w\.]+)\s*;\s*assembly\s*=\s*([\w\.]+)\s*$", RegexOptions.Compiled);

	public void AddNamespace(String prefix, String value)
	{
		if (_namespaces.ContainsKey(prefix))
			return;
		if (value == "http://schemas.microsoft.com/winfx/2006/xaml")
		{
			// xaml namespace for x:Key, etc
			_namespaces.Add(prefix, NamespaceDefinition.SpecialNamespace());
			return;
		}
		var nsddef = IsCustomNamespace(value);
		if (nsddef != null)
		{
			var nsd = new NamespaceDefinition(nsddef.Namespace, Assembly.Load(nsddef.Assembly))
			{
				IsCamelCase = nsddef.IsCamelCase,
				IsSkip = nsddef.IsSkip
			};
			_namespaces.Add(prefix, nsd);
			return;
		}
		var match = _namespaceRegEx.Match(value);
		if (match.Groups.Count == 3)
		{
			var assemblyName = match.Groups[2].Value.Trim();
			var nameSpace = match.Groups[1].Value.Trim();
			var nsd = new NamespaceDefinition(nameSpace, Assembly.Load(assemblyName));
			_namespaces.Add(prefix, nsd);
		}
	}

	public MarkupExtension ParseExtension(String value)
	{
		var node = ExtensionParser.Parse(this, value);
		var ext = BuildNode(node);
		if (ext is MarkupExtension markExt)
			return markExt;
		throw new XamlException($"Element '{value}' is not MarkupExtension");
	}

	private static PropertyDescriptor BuildPropertyDefinition(PropertyInfo propInfo)
	{
		Type propType = propInfo.PropertyType;
		Func<Object>? ctor = null;
		TypeConverter? typeConverter = null;

		if (propType != typeof(String))
		{
			var propCtor = propType.GetConstructor(Array.Empty<Type>());
			if (propCtor != null)
				ctor = Expression.Lambda<Func<Object>>(
					Expression.New(propCtor)
				).Compile();
			var conv = propType.GetCustomAttribute<TypeConverterAttribute>();
			if (conv != null)
			{
				var descrType = Type.GetType(conv.ConverterTypeName);
				if (descrType == null)
					throw new XamlException($"Invalid converter {conv.ConverterTypeName}");
				typeConverter = Activator.CreateInstance(descrType) as TypeConverter;
			}
		}

		var collLambdas = LambdaHelper.AddCollectionMethods(propType);

		return new PropertyDescriptor(propInfo)
		{
			Constructor = ctor,
			TypeConverter = typeConverter,
			AddMethod = collLambdas.AddCollection,
			AddDictionaryMethod = collLambdas.AddDictionary
		};
	}

	TypeDescriptor BuildTypeDescriptor(ClassNamePair namePair)
	{
		if (!_namespaces.TryGetValue(namePair.Prefix, out NamespaceDefinition? nsd))
			throw new XamlException($"Namespace {namePair.Namespace} not found");
		String typeName = $"{namePair.Namespace}.{namePair.ClassName}";
		var nodeType = nsd.Assembly.GetType(typeName);
		if (nodeType == null)
			throw new XamlException($"Class {namePair.Namespace}.{namePair.ClassName} not found");
		Func<Object>? constructor = null;
		Func<String, Object>? constructorStr = null;
		Func<IServiceProvider, Object>? constructorService = null;
		var ctor0 = nodeType.GetConstructor(Array.Empty<Type>());
		if (ctor0 != null)
			constructor = Expression.Lambda<Func<Object>>(Expression.New(nodeType)).Compile();
		var ctorStr = nodeType.GetConstructor(new Type[] { typeof(String) });
		if (ctorStr != null)
		{
			var prm = Expression.Parameter(typeof(String));
			constructorStr = Expression.Lambda<Func<String, Object>>(
				Expression.New(ctorStr, prm), 
				prm
			).Compile();
		}
		var ctorService = nodeType.GetConstructor(new Type[] { typeof(IServiceProvider) });
		if (ctorService != null)
		{
			var prm = Expression.Parameter(typeof(IServiceProvider));
			constructorService = Expression.Lambda<Func<IServiceProvider, Object>>(
				Expression.New(ctorService, prm),
				prm
			).Compile();
		}

		var props = nodeType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

		Action<Object,Object>? addCollection = null;
		Action<Object, String, Object>? addDictionary = null;

		var contentProperty = nodeType.GetCustomAttribute<ContentPropertyAttribute>()?.Name;

		if (contentProperty != null)
		{
			var contProp = props.Where(x => x.Name == contentProperty).FirstOrDefault();
			if (contProp == null)
				throw new XamlException($"Property {contentProperty} not found in type {typeName}");
			var collMethods = LambdaHelper.AddCollectionMethods(contProp.PropertyType);
			addCollection = collMethods.AddCollection;
			addDictionary = collMethods.AddDictionary;
		}
		else
		{
			var collMethods = LambdaHelper.AddCollectionMethods(nodeType);
			addCollection = collMethods.AddCollection;
			addDictionary = collMethods.AddDictionary;
		}

		var propDefs = new Dictionary<String, PropertyDescriptor>();
		foreach (var prop in props)
			propDefs.Add(prop.Name, BuildPropertyDefinition(prop));

		return new TypeDescriptor(nodeType, typeName, propDefs, BuildNode)
		{
			IsCamelCase = namePair.IsCamelCase,
			Constructor = constructor,
			ConstructorString = constructorStr,
			ConstructorService = constructorService,
			ContentProperty = contentProperty,
			AddCollection = addCollection,
			AddDictionary = addDictionary,
			AttachedProperties = BuildAttachedProperties(nodeType)
		};
	}


	private Dictionary<String, AttachedPropertyDescriptor>? BuildAttachedProperties(Type nodeType)
	{
		if (AttachedPropertyManager != null)
			return null;
		var propList = nodeType.GetCustomAttribute<AttachedPropertiesAttribute>()?.List;
		if (propList == null)
			return null;
		var lst = new Dictionary<String, AttachedPropertyDescriptor>();
		foreach (var prop in propList.Split(','))
		{
			var pName = prop.Trim();
			var elem = Expression.Parameter(typeof(Object));
			var val = Expression.Parameter(typeof(Object));
			var mtd = nodeType.GetMethod($"Set{pName}", BindingFlags.Static | BindingFlags.Public);
			if (mtd == null)
				throw new XamlException($"Invalid attached property {prop} for type {nodeType}");
				
			var args = mtd.GetParameters();
			var propValueType = args[1].ParameterType;
			TypeConverter? typeConverter = null;

			var lambda = Expression.Lambda<Action<Object, Object>>(
				Expression.Call(mtd,
					elem,
					Expression.Convert(
						val,
						propValueType
					)
				),
				elem,
				val
			).Compile();

			var tcAttr = propValueType.GetCustomAttribute<TypeConverterAttribute>();
			if (tcAttr != null)
			{
				var convType = Type.GetType(tcAttr.ConverterTypeName);
				if (convType == null)
					throw new XamlException($"Converter type '{tcAttr.ConverterTypeName}' not found");
				typeConverter = Activator.CreateInstance(convType) as TypeConverter;
			}

			lst.Add(pName, new AttachedPropertyDescriptor(propValueType, lambda, typeConverter));
		}
		return lst;
	}

	public (String Name, Boolean Special) QualifyPropertyName(String name)
	{
		if (!name.Contains(':'))
			return (name, false);
		var name2 = name.Split(':');
		if (!_namespaces.TryGetValue(name2[0], out NamespaceDefinition? def))
			throw new XamlException($"Namespace '{name2[0]}' not found");
		if (def.IsCamelCase)
			return (name2[1].ToPascalCase(), false);
		if (def.IsSpecial)
			return (name2[1], true);
		return (name2[1], false);
	}

	public TypeDescriptor? GetNodeDescriptor(String typeName)
	{
		String nsKey = String.Empty;
		if (typeName.Contains(':'))
		{
			// type with namespace
			var pair = typeName.Split(':');
			nsKey = pair[0];
			typeName = pair[1];
		}
		if (_namespaces.TryGetValue(nsKey, out NamespaceDefinition? nsd))
		{
			if (nsd.IsSkip)
				return null;
			if (nsd.IsCamelCase)
			{
				// file: camelCase
				// code: PascalCase
				typeName = typeName.ToPascalCase();
			}
			var className = new ClassNamePair(
				Prefix: nsKey, 
				Namespace: nsd.Namespace, 
				ClassName: CheckAlias(typeName), 
				IsCamelCase: nsd.IsCamelCase);
			return _typeCache.GetOrAdd(className, BuildTypeDescriptor);
		}
		else
			throw new XamlException($"Namespace '{nsKey}' not found");
	}

	String CheckAlias(String name)
	{
		if (_options == null || _options.Aliases == null)
			return name;
		if (_options.Aliases.TryGetValue(name, out String? outName))
			return outName;
		return name;
	}


	public Object? BuildNode(XamlNode node)
	{
		var nd = GetNodeDescriptor(node.Name);
		if (nd == null)
			return null;
		Object? obj = null;
		if (node.ConstructorArgument != null && nd.ConstructorString != null)
			obj = nd.ConstructorString(node.ConstructorArgument);
		else if (nd.ConstructorService != null)
			obj = nd.ConstructorService(_serviceProvider);
		else if (nd.Constructor != null)
			obj = nd.Constructor();
		else if (node.IsSimpleContent)
			return Convert.ChangeType(node.SimpleContent, nd.NodeType); // Simple string ????
		else
			throw new XamlException("Invalid build node argument");

		if (node.IsSimpleContent)
			nd.SetTextContent(obj, node.SimpleContent);

		foreach (var (propKey, propValue) in node.Properties)
		{
			if (propValue is SpecialPropertyDescriptor)
				continue;
			if (propValue is MarkupExtension markup)
			{
				// deferred!
				var propInfo = nd.GetPropertyInfo(propKey);
				node.Extensions.Add(new XamlExtensionElem(propInfo, markup));
			}
			else
				nd.SetPropertyValue(obj, propKey, propValue);
		}
		if (node.HasChildren)
		{
			foreach (var ch in node.Children.Value)
			{
				Object? chObj;
				if (ch is XamlTextContent chXaml)
					chObj = chXaml.Text;
				else
					chObj = BuildNode(ch);
				if (chObj != null)
					nd.AddChildren(obj, chObj, ch);
			}
		}

		ProcessExtensions(node.Extensions, obj);
		node.ProcessAttachedProperties(this, obj);

		if (obj is ISupportInitialize init) {
			init.BeginInit();
			init.EndInit();
		}
		return obj;
	}

	public void ExecuteDeferred()
	{
		if (!_deferExec.IsValueCreated)
			return;
		foreach (var a in _deferExec.Value)
			a();
	}

	public void ProcessExtensions(List<XamlExtensionElem> elems, Object target)
	{
		if (elems == null || elems.Count == 0)
			return;
		var valueTarget = _serviceProvider.ProvideValueTarget;
		foreach (var ext in elems)
		{
			_deferExec.Value.Add(() =>
			{
				valueTarget.TargetObject = target;
				valueTarget.TargetProperty = ext.PropertyInfo;
				var val = ext.Element.ProvideValue(_serviceProvider);
				if (val != null)
					ext.PropertyInfo.SetValue(target, val);
			});
		}
	}
}

