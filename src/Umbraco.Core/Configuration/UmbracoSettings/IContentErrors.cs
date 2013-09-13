using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IContentErrors
    {
        IEnumerable<IContentErrorPage> Error404Collection { get; }
    }
}