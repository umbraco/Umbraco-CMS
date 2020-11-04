using System.Collections.Generic;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Events
{
    [UmbracoVolatile]
    public interface IDeletingMediaFilesEventArgs
    {
        List<string> MediaFilesToDelete { get; }
    }
}
