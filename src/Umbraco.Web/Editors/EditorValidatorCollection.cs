using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Editors
{
    internal class EditorValidatorCollection : BuilderCollectionBase<IEditorValidator>
    {
        public EditorValidatorCollection(IEnumerable<IEditorValidator> items)
            : base(items)
        { }
    }
}
