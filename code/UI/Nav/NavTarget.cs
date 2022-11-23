using System;
using System.Linq;

namespace CrazyEights.UI;

public class NavTargetAttribute : Attribute, Sandbox.ITypeAttribute
{
    public string Url { get; set; }

    public Type TargetType { get; set; }

    public NavTargetAttribute(string url)
    {
        Url = url;
    }

    public static NavTargetAttribute Get(string url)
    {
        return TypeLibrary.GetAttributes<NavTargetAttribute>().Where(x => x.Url == url).FirstOrDefault();
    }
}
