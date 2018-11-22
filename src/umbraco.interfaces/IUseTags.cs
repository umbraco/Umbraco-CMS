using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.interfaces
{
    public interface IUseTags
    {
        string Group { get; }
        void RemoveTag(int nodeId, string tag);
        void RemoveTagsFromNode(int nodeId);
        void AddTag(string tag);
        void AddTagToNode(int nodeId, string tag);
        List<ITag> GetTagsFromNode(int nodeId);
        List<ITag> GetAllTags();
    }
}
