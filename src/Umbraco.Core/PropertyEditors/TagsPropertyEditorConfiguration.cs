using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    public class TagsPropertyEditorConfiguration
    {
        public string Group { get; set; } = "default";

        public TagCacheStorageType StorageType { get; set; } = TagCacheStorageType.Csv;
    }
}
