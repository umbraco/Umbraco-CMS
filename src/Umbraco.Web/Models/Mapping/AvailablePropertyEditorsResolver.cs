using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class AvailablePropertyEditorsResolver : ValueResolver<IDataTypeDefinition, IEnumerable<PropertyEditorBasic>>
    {
        protected override IEnumerable<PropertyEditorBasic> ResolveCore(IDataTypeDefinition source)
        {
            return Current.PropertyEditors
                .OrderBy(x => x.Name)
                .Select(Mapper.Map<PropertyEditorBasic>);
        }
    }
}