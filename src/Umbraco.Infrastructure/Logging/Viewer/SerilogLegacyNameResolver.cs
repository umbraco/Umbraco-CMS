using System.Diagnostics.CodeAnalysis;
using Serilog.Expressions;

namespace Umbraco.Cms.Infrastructure.Logging.Viewer;

/// <summary>
///     Inherits Serilog's StaticMemberNameResolver to ensure we get same functionality
///     Of easily allowing any static methods definied in the passed in class/type
///     To extend as functions to use for filtering logs such as Has() and any other custom ones
/// </summary>
public class SerilogLegacyNameResolver : StaticMemberNameResolver
{
    public SerilogLegacyNameResolver(Type type)
        : base(type)
    {
    }

    /// <summary>
    ///     Allows us to fix the gap from migrating away from Serilog.Filters.Expressions
    ///     So we can still support the more verbose built in property names such as
    ///     Exception, Level, MessageTemplate etc
    /// </summary>
    public override bool TryResolveBuiltInPropertyName(string alias, [MaybeNullWhen(false)] out string target)
    {
        target = alias switch
        {
            "Exception" => "x",
            "Level" => "l",
            "Message" => "m",
            "MessageTemplate" => "mt",
            "Properties" => "p",
            "Timestamp" => "t",
            _ => null,
        };

        return target != null;
    }
}
