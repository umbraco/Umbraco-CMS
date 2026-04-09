using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Extensions;

internal static class InstanceIdentifiableExtensions
{
    /// <summary>
    /// Gets debug information for the specified <see cref="IInstanceIdentifiable"/> instance, including a shortened instance ID and the thread ID where it was created.
    /// </summary>
    /// <param name="instance">The instance to get debug information from.</param>
    /// <returns>
    /// A string containing debug information about the instance in the format <c>(id: [first 8 chars of InstanceId] from thread: [CreatedThreadId])</c>,
    /// or <c>"(NULL)"</c> if the instance is <c>null</c>.
    /// </returns>
    public static string GetDebugInfo(this IInstanceIdentifiable? instance)
    {
        if (instance == null)
        {
            return "(NULL)";
        }

        return $"(id: {instance.InstanceId.ToString("N").Substring(0, 8)} from thread: {instance.CreatedThreadId})";
    }
}
