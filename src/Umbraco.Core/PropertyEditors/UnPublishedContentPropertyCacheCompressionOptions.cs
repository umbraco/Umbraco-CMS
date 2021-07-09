using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Compress large, non published text properties
    /// </summary>
    internal class UnPublishedContentPropertyCacheCompressionOptions : IPropertyCacheCompressionOptions
    {
        public bool IsCompressed(IReadOnlyContentBase content, PropertyType propertyType, IDataEditor dataEditor, bool published)
        {
            if (!published && propertyType.SupportsPublishing && propertyType.ValueStorageType == ValueStorageType.Ntext)
            {
                //Only compress non published content that supports publishing and the property is text
                return true;
            }
            return false;
        }
    }
}
