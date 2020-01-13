using System.Globalization;

namespace Umbraco.Tests.Shared.Builders.Interfaces
{
    public interface IWithCultureInfoBuilder
    {
        CultureInfo CultureInfo { get; set; }
    }
}
