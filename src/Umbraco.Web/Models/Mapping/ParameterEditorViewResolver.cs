using System;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.Models.Mapping
{
    internal class ParameterEditorViewResolver : ValueResolver<IMacroProperty, string>
    {
        protected override string ResolveCore(IMacroProperty source)
        {
            var paramEditor = ParameterEditorResolver.Current.GetByAlias(source.EditorAlias);
            if (paramEditor == null)
            {
                throw new InvalidOperationException("Could not resolve macro parameter editor: " + source.EditorAlias);
            }
            return paramEditor.ValueEditor.View;
        }
    }
}