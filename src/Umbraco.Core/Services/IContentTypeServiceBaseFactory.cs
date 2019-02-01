using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IContentTypeServiceBaseFactory
    {
        IContentTypeServiceBase Create(IContentBase contentBase);

    }
}
