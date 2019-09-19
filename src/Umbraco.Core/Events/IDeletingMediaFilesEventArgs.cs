using System.Collections.Generic;

namespace Umbraco.Core.Events
{
    internal interface IDeletingMediaFilesEventArgs
    {
        List<string> MediaFilesToDelete { get; }
    }
}