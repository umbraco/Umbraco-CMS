using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace Umbraco.Core.PropertyEditors
{
    internal abstract class EditorModel<TValueModel> where TValueModel : IValueModel, new()
    {
        protected EditorModel()
        {
            // Set the UI Elements collection to an empty list
            //UIElements = new List<UIElement>();
        }

        [ReadOnly(true)]
        public virtual bool ShowUmbracoLabel
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a list of UI Elements for the property editor.
        /// </summary>
        //[ScaffoldColumn(false)]
        //public virtual IList<UIElement> UIElements { get; protected internal set; }

        private ModelMetadata _modelMetadata;

        /// <summary>
        /// Returns the meta data for the current editor model
        /// </summary>
        protected internal ModelMetadata MetaData
        {
            get
            {
                return _modelMetadata ?? (_modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => this, GetType()));
            }
        }

        /// <summary>
        /// Returns the serialized value for the PropertyEditor
        /// </summary>
        /// <returns></returns>
        public virtual IDictionary<string, object> GetSerializedValue()
        {
            var editableProps = MetaData.Properties.Where(x => x.ShowForEdit && !x.IsReadOnly);
            var d = new Dictionary<string, object>();
            foreach (var p in editableProps)
            {
                //by default, we will not support complex modelled properties, developers will need to override
                //the GetSerializedValue method if they need support for this.
                if (p.IsComplexType)
                {
                    //TODO: We should magically support this
                    throw new NotSupportedException("The default serialization implementation of EditorModel does not support properties that are complex models");
                }

                d.Add(p.PropertyName, p.Model);
            }
            return d;
        }

        public virtual void SetModelValues(IDictionary<string, object> serializedVal)
        {
            if (serializedVal == null || serializedVal.Count == 0)
            {
                return;
            }

            var modelProperties = GetType().GetProperties();

            var editableProps = MetaData.Properties.Where(x => x.ShowForEdit && !x.IsReadOnly);

            foreach (var i in serializedVal)
            {
                if (i.Value == null)
                    continue;

                //get the property with the name
                var prop = editableProps.Where(x => x.PropertyName == i.Key).SingleOrDefault();
                if (prop != null)
                {
                    //set the property value
                    var toConverter = TypeDescriptor.GetConverter(prop.ModelType);
                    if (toConverter != null)
                    {
                        //get the model property for this property meta data to set its value
                        var propInfo = modelProperties.Where(x => x.Name == prop.PropertyName).Single();
                        object convertedVal;

                        //if value is already of the same type, just use the current value, otherwise try and convert it
                        if (i.Value.GetType() == propInfo.PropertyType)
                        {
                            convertedVal = i.Value;
                        }
                        else
                        {
                            try
                            {
                                convertedVal = toConverter.ConvertFrom(i.Value);
                            }
                            catch (NotSupportedException)
                            {
                                //this occurs when the converter doesn't know how, so we can try the opposite way as a last ditch effort
                                var fromConverter = TypeDescriptor.GetConverter(i.Value.GetType());
                                if (fromConverter == null)
                                {
                                    throw;
                                }
                                convertedVal = fromConverter.ConvertTo(i.Value, prop.ModelType);

                            }
                        }
                        propInfo.SetValue(this, convertedVal, null);

                    }
                }

            }
        }

        public virtual TValueModel GetValueModel()
        {
            var editableProps = MetaData.Properties.Where(x => x.ShowForEdit && !x.IsReadOnly);
            var d = new TValueModel();
            foreach (var p in editableProps)
            {
                //by default, we will not support complex modelled properties, developers will need to override
                //the GetSerializedValue method if they need support for this.
                //TODO Test if this exception is still valid
                if (p.IsComplexType)
                {
                    throw new NotSupportedException("The default serialization implementation of EditorModel does not support properties that are complex models");
                }

                var property = d.GetType().GetProperty(p.PropertyName);
                if(property != null)
                {
                    property.SetValue(d, p.Model, null);
                }
            }
            return d;
        }
    }
}