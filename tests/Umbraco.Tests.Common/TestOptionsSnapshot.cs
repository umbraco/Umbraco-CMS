using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Tests.Common;

public class TestOptionsSnapshot<T> : IOptionsSnapshot<T> where T : class
{
    public TestOptionsSnapshot(T value) => Value = value;
    public T Value { get; }
    public T Get(string name) => Value;
}
