using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class PreValueDisplayResolver : ValueResolver<IDataTypeDefinition, IEnumerable<PreValueFieldDisplay>>
    {
        protected override IEnumerable<PreValueFieldDisplay> ResolveCore(IDataTypeDefinition source)
        {
            var propEd = PropertyEditorResolver.Current.GetById(source.ControlId);
            if (propEd == null)
            {
                throw new InvalidOperationException("Could not find property editor with id " + source.ControlId);
            }

            return propEd.PreValueEditor.Fields.Select(Mapper.Map<PreValueFieldDisplay>);
        }
    }
}