using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    // fixme - now
    // this collection is maintained but never used - yes! never injected,
    // but Current.ParameterEditors is used
    // - in MacroMapperProfile
    // - in editMacro.aspx
    // this is providing parameter editors for macros
    // what's an IParameterEditor?
    //
    // fixme - now
    // - also, grid editor
    // - sort out the editors hierarchy
    // - then prevalues!
    //
    // namespace: Umbraco.Core.DataEditors
    //  .Validators
    //  .Editors
    //  .ValueConverters
    //
    // IDataEditor
    //  .Alias (unique identifier)
    //  .Name
    //  .IsPropertyValueEditor
    //  .IsMacroParameterEditor
    //  .ParseConfiguration(string config) : object
    //
    // I <from PropertyEditor>
    //  .Icon
    //  .Group
    //  .IsDeprecated
    //  .DefaultConfiguration : object
    //
    // DataEditor
    //  .ValueEditor : IValueEditor
    //    .View : string
    //  .ConfigurationEditor : IDataEditorConfigurationEditor
    //
    // IDataType
    //  .EditorAlias
    //  .DataStorage (DataStorageType.Text, .Varchar, .Int, ...)
    //  .Configuration : object
    //
    // ParameterValueEditor : IValueEditor
    //  .View : string
    //
    // PropertyValueEditor : IValueEditor
    //  .View : string
    //  .HideLabel : bool
    //  .ValueType (ValueTypes.Xml, .String...)
    //  .Validators : IPropertyValidator*
    //  .convert...
    //
    // IDataEditorConfigurationEditor
    //  .Fields : DataEditorConfigurationField*
    //  .FromEditor() - should receive an JObject - need to convert to true configuration
    //  .ToEditor() - in most cases, just pass the configuration object
    //
    // load
    //  read config field as string from db
    //  read alias field as string from db
    //  find data editor corresponding to alias
    //   not found = deserialize config as IDictionary<string, object>
    //   else = use editor to deserialize configuration
    //   PROBLEM should be a POCO not a dictionary anymore

    public class ParameterEditorCollection : BuilderCollectionBase<IParameterEditor>
    {
        public ParameterEditorCollection(IEnumerable<IParameterEditor> items)
            : base(items)
        { }

        // note: virtual so it can be mocked
        public virtual IParameterEditor this[string alias]
            => this.SingleOrDefault(x => x.Alias == alias);

        public virtual bool TryGet(string alias, out IParameterEditor editor)
        {
            editor = this.FirstOrDefault(x => x.Alias == alias);
            return editor != null;
        }
    }
}
