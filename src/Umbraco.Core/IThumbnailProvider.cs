using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core
{
    internal interface IThumbnailProvider
    {
        int Priority { get; }
        bool CanProvideThumbnail(string fileUrl);
        string GetThumbnailUrl(string fileUrl);
    }

}
