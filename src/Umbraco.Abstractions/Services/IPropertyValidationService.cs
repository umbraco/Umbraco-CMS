using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IPropertyValidationService
    {
        bool IsPropertyDataValid(IContent content, out IProperty[] invalidProperties, CultureImpact impact);
        bool IsPropertyValid(IProperty property, string culture = "*", string segment = "*");
    }
}