﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.RelatedLinksAlias, "Related links", "relatedlinks", ValueType ="JSON", Icon="icon-thumbnail-list", Group="pickers")]
    public class RelatedLinksPropertyEditor : PropertyEditor
    {
    }
}
