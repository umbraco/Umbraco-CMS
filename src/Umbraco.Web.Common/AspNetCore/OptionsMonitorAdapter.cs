using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Web.Common.AspNetCore;

/// <summary>
///     HACK: OptionsMonitor but without the monitoring, hopefully temporary.
///     This is just used so we can get an AspNetCoreHostingEnvironment to
///     build a TypeLoader long before ServiceProvider is built.
/// </summary>
[Obsolete("Please let the container wire up a real OptionsMonitor for you")]
internal class OptionsMonitorAdapter<T> : IOptionsMonitor<T>
    where T : class, new()
{
    public OptionsMonitorAdapter(T inner) => CurrentValue = inner ?? throw new ArgumentNullException(nameof(inner));

    public T CurrentValue { get; }

    public T Get(string name) => CurrentValue;

    public IDisposable OnChange(Action<T, string> listener) => throw new NotImplementedException();
}
