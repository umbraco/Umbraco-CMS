using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.IO
{
    [FileSystemProvider("media")]
    internal interface IMediaFileSystem : IFileSystem
    { }
}
