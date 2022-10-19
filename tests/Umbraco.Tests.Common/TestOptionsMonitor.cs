using System;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Tests.Common;

public class TestOptionsMonitor<T> : IOptionsMonitor<T> where T : class
{
    public TestOptionsMonitor(T currentValue) => CurrentValue = currentValue;

    public T Get(string name) => CurrentValue;

    public IDisposable OnChange(Action<T, string> listener) => null;

    public T CurrentValue { get; }
}
