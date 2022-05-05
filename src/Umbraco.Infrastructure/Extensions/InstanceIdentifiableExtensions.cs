using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Extensions;

internal static class InstanceIdentifiableExtensions
{
    public static string GetDebugInfo(this IInstanceIdentifiable? instance)
    {
        if (instance == null)
        {
            return "(NULL)";
        }

        return $"(id: {instance.InstanceId.ToString("N")[..8]} from thread: {instance.CreatedThreadId})";
    }
}
