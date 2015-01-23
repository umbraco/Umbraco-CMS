using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.RelatedLinksAlias, "Related links", "relatedlinks", ValueType ="JSON")]
    public class RelatedLinksPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public RelatedLinksPropertyEditor(ILogger logger) : base(logger)
        {
        }
    }
}
