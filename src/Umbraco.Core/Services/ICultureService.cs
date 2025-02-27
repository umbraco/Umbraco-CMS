using System.Globalization;

namespace Umbraco.Cms.Core.Services;

public interface ICultureService
{
    CultureInfo[] GetValidCultureInfos();
}
