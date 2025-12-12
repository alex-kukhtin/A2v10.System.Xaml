using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.System.Xaml;

public class XamlWriterService
{
    public String GetXaml(Object obj)
    {
        var xw = new XamlWriter();
        return xw.GetXaml(obj);
    }
}
