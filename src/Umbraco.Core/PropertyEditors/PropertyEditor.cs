using System;
using System.Collections.Generic;
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
    public class PropertyEditor
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
                Id = Guid.Parse(_attribute.Id);
                Name = _attribute.Name;                
            }
        }

        /// <summary>
        /// These are assigned by default normally based on property editor attributes or manifest definitions,
        /// developers have the chance to override CreateValueEditor if they don't want to use the pre-defined instance
        /// </summary>
        internal ValueEditor ManifestDefinedValueEditor = null;

        /// <summary>
        /// These are assigned by default normally based on property editor attributes or manifest definitions,
        /// developers have the chance to override CreatePreValueEditor if they don't want to use the pre-defined instance
        /// </summary>
        internal PreValueEditor ManifestDefinedPreValueEditor = null;

        /// <summary>
        /// The id  of the property editor
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public Guid Id { get; internal set; }
        
        /// <summary>
        /// The name of the property editor
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; internal set; }

        [JsonProperty("editor", Required = Required.Always)]        
        public ValueEditor ValueEditor
        {
            get { return CreateValueEditor(); }
        }

        [JsonProperty("prevalues")]
        public PreValueEditor PreValueEditor
        {
            get { return CreatePreValueEditor(); }
        }

        [JsonProperty("defaultConfig")]
        public virtual IDictionary<string, object> DefaultPreValues { get; set; }

        /// <summary>
        /// Creates a value editor instance
        /// </summary>
        /// <returns></returns>
        protected virtual ValueEditor CreateValueEditor()
        {
            if (ManifestDefinedValueEditor != null)
            {
                //detect if the view is a virtual path (in most cases, yes) then convert it
                if (ManifestDefinedValueEditor.View.StartsWith("~/"))
                {
                    ManifestDefinedValueEditor.View = IOHelper.ResolveUrl(ManifestDefinedValueEditor.View);
                }
                return ManifestDefinedValueEditor;
            }

            //create a new editor
            var editor = new ValueEditor();

            if (_attribute.EditorView.IsNullOrWhiteSpace())
            {
                throw new NotImplementedException("This method must be implemented if a view is not explicitly set");
            }

            editor.View = _attribute.EditorView;
            editor.ValueType = _attribute.ValueType;
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
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as PropertyEditor;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}