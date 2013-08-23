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
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        internal PropertyEditor()
        {
            StaticallyDefinedValueEditor = new ValueEditor();
            StaticallyDefinedPreValueEditor = new PreValueEditor();

            //assign properties based on the attribute if it is found
            var att = GetType().GetCustomAttribute<PropertyEditorAttribute>(false);
            if (att != null)
            {
                Id = Guid.Parse(att.Id);
                Name = att.Name;
                
                StaticallyDefinedValueEditor.ValueType = att.ValueType;
                StaticallyDefinedValueEditor.View = att.EditorView;
            }
        }

        /// <summary>
        /// These are assigned by default normally based on property editor attributes or manifest definitions,
        /// developers have the chance to override CreateValueEditor if they don't want to use the pre-defined instance
        /// </summary>
        internal ValueEditor StaticallyDefinedValueEditor = null;

        /// <summary>
        /// These are assigned by default normally based on property editor attributes or manifest definitions,
        /// developers have the chance to override CreatePreValueEditor if they don't want to use the pre-defined instance
        /// </summary>
        internal PreValueEditor StaticallyDefinedPreValueEditor = null;

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

        [JsonProperty("preValueEditor")]
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
            if (StaticallyDefinedValueEditor != null && !StaticallyDefinedValueEditor.View.IsNullOrWhiteSpace())
            {
                //detect if the view is a virtual path (in most cases, yes) then convert it
                if (StaticallyDefinedValueEditor.View.StartsWith("~/"))
                {
                    StaticallyDefinedValueEditor.View = IOHelper.ResolveUrl(StaticallyDefinedValueEditor.View);
                }

                return StaticallyDefinedValueEditor;
            }
            throw new NotImplementedException("This method must be implemented if a view is not explicitly set");
        }

        /// <summary>
        /// Creates a pre value editor instance
        /// </summary>
        /// <returns></returns>
        protected virtual PreValueEditor CreatePreValueEditor()
        {            
            if (StaticallyDefinedPreValueEditor != null)
            {
                foreach (var f in StaticallyDefinedPreValueEditor.Fields)
                {
                    //detect if the view is a virtual path (in most cases, yes) then convert it
                    if (f.View.StartsWith("~/"))
                    {
                        f.View = IOHelper.ResolveUrl(f.View);
                    }    
                }                
            }
            return StaticallyDefinedPreValueEditor;
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