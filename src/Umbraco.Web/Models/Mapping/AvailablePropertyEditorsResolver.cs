using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class AvailablePropertyEditorsResolver : ValueResolver<IDataTypeDefinition, IEnumerable<PropertyEditorBasic>>
    {
        protected override IEnumerable<PropertyEditorBasic> ResolveCore(IDataTypeDefinition source)
        {
            return PropertyEditorResolver.Current.PropertyEditors
                .OrderBy(x => x.Name)
                .Select(Mapper.Map<PropertyEditorBasic>);
        }
    }
}