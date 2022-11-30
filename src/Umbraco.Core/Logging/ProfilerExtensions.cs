namespace Umbraco.Cms.Core.Logging;

internal static class ProfilerExtensions
{
    /// <summary>
    ///     Gets an <see cref="IDisposable" /> that will time the code between its creation and disposal,
    ///     prefixing the name of the step with a reporting type name.
    /// </summary>
    /// <typeparam name="T">The reporting type.</typeparam>
    /// <param name="profiler">The profiler.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>A step.</returns>
    /// <remarks>The returned <see cref="IDisposable" /> is meant to be used within a <c>using (...) {{ ... }}</c> block.</remarks>
    internal static IDisposable? Step<T>(this IProfiler profiler, string name)
    {
        if (profiler == null)
        {
            throw new ArgumentNullException(nameof(profiler));
        }

        return profiler.Step(typeof(T), name);
    }

    /// <summary>
    ///     Gets an <see cref="IDisposable" /> that will time the code between its creation and disposal,
    ///     prefixing the name of the step with a reporting type name.
    /// </summary>
    /// <param name="profiler">The profiler.</param>
    /// <param name="reporting">The reporting type.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>A step.</returns>
    /// <remarks>The returned <see cref="IDisposable" /> is meant to be used within a <c>using (...) {{ ... }}</c> block.</remarks>
    internal static IDisposable? Step(this IProfiler profiler, Type reporting, string name)
    {
        if (profiler == null)
        {
            throw new ArgumentNullException(nameof(profiler));
        }

        if (reporting == null)
        {
            throw new ArgumentNullException(nameof(reporting));
        }

        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        return profiler.Step($"[{reporting.Name}] {name}");
    }
}
