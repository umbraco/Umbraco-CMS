using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Umbraco.Core.Models.Blocks
{
    // TODO: Rename this, we don't want to use the name "Helper"
    // TODO: What is this? This requires code docs
    // TODO: This is not used publicly at all - therefore it probably shouldn't be public
    // TODO: These could easily be abstract methods on the underlying BlockEditorPropertyEditor instead
    public interface IBlockEditorDataHelper
    {
        // TODO: Does this abstraction need a reference to JObject? Maybe it does but ideally it doesn't
        IEnumerable<IBlockReference> GetBlockReferences(JObject layout);
        bool IsEditorSpecificPropertyKey(string propertyKey);
    }
}
