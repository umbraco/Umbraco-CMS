using System;
using Newtonsoft.Json;
using Umbraco.Core.IO;

namespace Umbraco.Core.PropertyEditors
{
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
                StaticallyDefinedPreValueEditor.View = att.PreValueEditorView;
            }
        }

        internal ValueEditor StaticallyDefinedValueEditor = null;
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

        [JsonProperty("editor")]        
        public ValueEditor ValueEditor
        {
            get { return CreateValueEditor(); }
        }

        [JsonProperty("preValueEditor")]
        public PreValueEditor PreValueEditor
        {
            get { return CreatePreValueEditor(); }
        }

        //TODO: Now we need to implement a couple of methods for saving the data for editors and pre-value editors
        // generally we can handle that automatically in this base class but people should be allowed to override 
        // it so they can perform custom operations on saving the data.
        // This also means that we should have methods for formatting the data upon retreival from the repository
        // essentially we need serialization and deserialization methods to override.

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