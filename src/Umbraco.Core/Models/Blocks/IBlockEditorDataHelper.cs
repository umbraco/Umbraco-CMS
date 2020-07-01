using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Umbraco.Core.Models.Blocks
{
    public interface IBlockEditorDataHelper
    {
        IEnumerable<IBlockReference> GetBlockReferences(JObject layout);
        bool IsEditorSpecificPropertyKey(string propertyKey);
    }
}
