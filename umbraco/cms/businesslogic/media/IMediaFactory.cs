using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.BusinessLogic;

namespace umbraco.cms.businesslogic.media
{
    public interface IMediaFactory
    {
        List<string> Extensions { get; }
        int Priority { get; }

        bool CanCreateMedia(int parentNodeId, PostedMediaFile postedFile, User user);
        Media CreateMedia(int parentNodeId, PostedMediaFile postedFile, User user);
    }
}
