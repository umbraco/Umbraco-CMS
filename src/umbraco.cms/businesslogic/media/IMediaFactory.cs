using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.BusinessLogic;

namespace umbraco.cms.businesslogic.media
{
    [Obsolete("This interface is no longer used and will be removed from the codebase in future versions")]
    public interface IMediaFactory
    {
        List<string> Extensions { get; }
        int Priority { get; }

        bool CanHandleMedia(int parentNodeId, PostedMediaFile postedFile, User user);

        Media HandleMedia(int parentNodeId, PostedMediaFile postedFile, User user);

        [Obsolete("Use HandleMedia(int, PostedMediaFile, User) and set the ReplaceExisting property on PostedMediaFile instead")]
        Media HandleMedia(int parentNodeId, PostedMediaFile postedFile, User user, bool replaceExisting);
    }
}
