using System;
using System.ComponentModel;
using System.Linq;
using Nuke.Common.Tooling;

/// <summary>
/// https://github.com/nuke-build/nuke/commit/c653e06c4f4399475ca550e0507470325ee169c1
/// </summary>
[TypeConverter(typeof(TypeConverter<Configuration>))]
public class Configuration : Enumeration
{
    public static Configuration Debug = new Configuration { Value = nameof(Debug) };
    public static Configuration Release = new Configuration { Value = nameof(Release) };

    public static implicit operator string(Configuration configuration)
    {
        return configuration.Value;
    }
}