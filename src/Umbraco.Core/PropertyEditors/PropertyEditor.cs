using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Basic definition of a property editor
    /// </summary>
    /// <remarks>
    /// The Json serialization attributes are required for manifest property editors to work
    /// </remarks>
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public class PropertyEditor : IParameterEditor
    {
        private readonly PropertyEditorAttribute _attribute;

        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public PropertyEditor()             
        {
            //assign properties based on the attribute if it is found
            _attribute = GetType().GetCustomAttribute<PropertyEditorAttribute>(false);
            if (_attribute != null)
            {
                //set the id/name from the attribute
                Alias = _attribute.Alias;
                Name = _attribute.Name;
                IsParameterEditor = _attribute.IsParameterEditor;
            }
        }

        /// <summary>
        /// These are assigned by default normally based on property editor attributes or manifest definitions,
        /// developers have the chance to override CreateValueEditor if they don't want to use the pre-defined instance
        /// </summary>
        internal PropertyValueEditor ManifestDefinedPropertyValueEditor = null;

        /// <summary>
        /// These are assigned by default normally based on property editor attributes or manifest definitions,
        /// developers have the chance to override CreatePreValueEditor if they don't want to use the pre-defined instance
        /// </summary>
        internal PreValueEditor ManifestDefinedPreValueEditor = null;

        /// <summary>
        /// Boolean flag determining if this can be used as a parameter editor
        /// </summary>
        [JsonProperty("isParameterEditor")]
        public bool IsParameterEditor { get; internal set; }

        /// <summary>
        /// The id  of the property editor
        /// </summary>
        [JsonProperty("alias", Required = Required.Always)]
        public string Alias { get; internal set; }

        /// <summary>
        /// The name of the property editor
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; internal set; }

        [JsonProperty("editor", Required = Required.Always)]        
        public PropertyValueEditor ValueEditor
        {
            get { return CreateValueEditor(); }
        }

        [JsonIgnore]
        IValueEditor IParameterEditor.ValueEditor
        {
            get { return ValueEditor; }
        }

        [JsonProperty("prevalues")]
        public PreValueEditor PreValueEditor
        {
            get { return CreatePreValueEditor(); }
        }

        [JsonProperty("defaultConfig")]
        public virtual IDictionary<string, object> DefaultPreValues { get; set; }

        [JsonIgnore]
        IDictionary<string, object> IParameterEditor.Configuration
        {
            get { return DefaultPreValues; }
        }

        /// <summary>
        /// Creates a value editor instance
        /// </summary>
        /// <returns></returns>
        protected virtual PropertyValueEditor CreateValueEditor()
        {
            if (ManifestDefinedPropertyValueEditor != null)
            {
                //detect if the view is a virtual path (in most cases, yes) then convert it
                if (ManifestDefinedPropertyValueEditor.View.StartsWith("~/"))
                {
                    ManifestDefinedPropertyValueEditor.View = IOHelper.ResolveUrl(ManifestDefinedPropertyValueEditor.View);
                }
                return ManifestDefinedPropertyValueEditor;
            }

            //create a new editor
            var editor = new PropertyValueEditor();

            if (_attribute.EditorView.IsNullOrWhiteSpace())
            {
                throw new NotImplementedException("This method must be implemented if a view is not explicitly set");
            }

            editor.View = _attribute.EditorView.StartsWith("~/") ? IOHelper.ResolveUrl(_attribute.EditorView) : _attribute.EditorView;
            editor.ValueType = _attribute.ValueType;
            editor.HideLabel = _attribute.HideLabel;
            return editor;

        }

        /// <summary>
        /// Creates a pre value editor instance
        /// </summary>
        /// <returns></returns>
        protected virtual PreValueEditor CreatePreValueEditor()
        {      
            //This will not be null if it is a manifest defined editor
            if (ManifestDefinedPreValueEditor != null)
            {
                foreach (var f in ManifestDefinedPreValueEditor.Fields)
                {
                    //detect if the view is a virtual path (in most cases, yes) then convert it
                    if (f.View.StartsWith("~/"))
                    {
                        f.View = IOHelper.ResolveUrl(f.View);
                    }    
                }
                return ManifestDefinedPreValueEditor;
            }

            //There's no manifest, just return an empty one
            return new PreValueEditor();
        }

        protected bool Equals(PropertyEditor other)
        {
            return string.Equals(Alias, other.Alias);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PropertyEditor) obj);
        }

        public override int GetHashCode()
        {
            return Alias.GetHashCode();
        }

        /// <summary>
        /// Provides a summary of the PropertyEditor for use with the <see cref="DebuggerDisplayAttribute"/>.
        /// </summary>
        protected virtual string DebuggerDisplay()
        {
            return string.Format("Name: {0}, Alias: {1}, IsParameterEditor: {2}", Name, Alias, IsParameterEditor);
        }
    }
}