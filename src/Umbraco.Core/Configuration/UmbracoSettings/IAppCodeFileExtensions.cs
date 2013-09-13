using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IAppCodeFileExtensions
    {
        IEnumerable<IFileExtension> AppCodeFileExtensions { get; }
    }
}