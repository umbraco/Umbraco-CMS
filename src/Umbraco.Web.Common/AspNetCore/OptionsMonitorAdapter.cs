using System;
using Microsoft.Extensions.Options;

namespace Umbraco.Web.Common.AspNetCore
{
    /// <summary>
    /// OptionsMonitor but without the monitoring
    /// </summary>
    public class OptionsMonitorAdapter<T> : IOptionsMonitor<T> where T : class, new()
    {
        private readonly T _inner;

        public OptionsMonitorAdapter(T inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public T Get(string name)
        {
            return _inner;
        }

        public IDisposable OnChange(Action<T, string> listener)
        {
            throw new NotImplementedException();
        }

        public T CurrentValue => _inner;

    }
}
