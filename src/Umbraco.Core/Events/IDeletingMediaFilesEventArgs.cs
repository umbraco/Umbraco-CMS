using System.Collections.Generic;

namespace Umbraco.Core.Events
{
    public interface IDeletingMediaFilesEventArgs
    {
        List<string> MediaFilesToDelete { get; }
    }
}
