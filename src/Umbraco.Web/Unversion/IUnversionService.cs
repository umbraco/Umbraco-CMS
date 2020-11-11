using Umbraco.Core.Models;

namespace Umbraco.Web.Unversion
{
    public interface IUnversionService
    {
        void Unversion(IContent content);
    }
}

