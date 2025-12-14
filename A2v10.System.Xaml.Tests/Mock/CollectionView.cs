
using System.ComponentModel.DataAnnotations;

namespace A2v10.System.Xaml.Tests.Mock;

public enum RunMode
{
    Client,
    Server,
    ServerUrl
}

public enum SortDirection
{
    Asc,
    Desc,
}

public class SortDescription
{
    public String? Property { get; set; }

    public SortDirection Dir { get; set; }

}

public class GroupDescription
{
    public String? GroupBy { get; set; }
    public Boolean Count { get; set; }
    public Boolean Collapsed { get; set; }
    public String? Title { get; set; }
}


public class FilterItem
{
    public String? Property { get; set; }
    public Object? Value { get; set; }
    public DataType DataType { get; set; }
    public Boolean Persistent { get; set; }
}

public class FilterItems : List<FilterItem>
{

}

[ContentProperty("Items")]
public class FilterDescription
{
    public FilterItems Items { get; set; } = [];

}


[ContentProperty("Children")]
public class CollectionView : UIElementBase
{
    public Object? ItemsSource { get; set; }

    public Int32? PageSize { get; set; }

    public RunMode RunAt { get; set; }

    public UIElementCollection Children { get; set; } = [];

    public SortDescription? Sort { get; set; }

    public GroupDescription? GroupBy { get; set; }

    public FilterDescription? Filter { get; set; }

    public String? FilterDelegate { get; set; }

}
