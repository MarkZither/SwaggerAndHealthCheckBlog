using Nuke.Common.Tooling;
using System.ComponentModel;

/// <summary>
/// https://github.com/nuke-build/nuke/commit/c653e06c4f4399475ca550e0507470325ee169c1
/// </summary>
[TypeConverter(typeof(TypeConverter<Configuration>))]
public class Configuration : Enumeration
{
    /// <summary>
    /// debug build config
    /// </summary>
    public static Configuration Debug = new Configuration { Value = nameof(Debug) };
    /// <summary>
    /// release build config
    /// </summary>
    public static Configuration Release = new Configuration { Value = nameof(Release) };

    /// <summary>
    /// string represerntation of the enum
    /// </summary>
    /// <param name="configuration"></param>
    public static implicit operator string(Configuration configuration)
    {
        return configuration.Value;
    }
}
