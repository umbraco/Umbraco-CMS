using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    internal class BlockListConfigurationEditor : ConfigurationEditor<BlockListConfiguration>
    {
        public BlockListConfigurationEditor()
        {

        }

    }
}
