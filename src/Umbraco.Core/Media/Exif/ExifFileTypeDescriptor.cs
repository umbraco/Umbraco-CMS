using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Umbraco.Core.Media.Exif
{
    /// <summary>
    /// Provides a custom type descriptor for an ExifFile instance.
    /// </summary>
    internal sealed class ExifFileTypeDescriptionProvider : TypeDescriptionProvider
    {
        public ExifFileTypeDescriptionProvider()
            : this(TypeDescriptor.GetProvider(typeof(ImageFile)))
        {
        }

        public ExifFileTypeDescriptionProvider(TypeDescriptionProvider parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Gets a custom type descriptor for the given type and object.
        /// </summary>
        /// <param name="objectType">The type of object for which to retrieve the type descriptor.</param>
        /// <param name="instance">An instance of the type. Can be null if no instance was passed to the <see cref="T:System.ComponentModel.TypeDescriptor"/>.</param>
        /// <returns>
        /// An <see cref="T:System.ComponentModel.ICustomTypeDescriptor"/> that can provide metadata for the type.
        /// </returns>
        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return new ExifFileTypeDescriptor(base.GetTypeDescriptor(objectType, instance), instance);
        }
    }

    /// <summary>
    /// Expands ExifProperty objects contained in an ExifFile as seperate properties.
    /// </summary>
    internal sealed class ExifFileTypeDescriptor : CustomTypeDescriptor
    {
        ImageFile owner;

        public ExifFileTypeDescriptor(ICustomTypeDescriptor parent, object instance)
            : base(parent)
        {
            owner = (ImageFile)instance;
        }
        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }
        /// <summary>
        /// Returns a collection of property descriptors for the object represented by this type descriptor.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.ComponentModel.PropertyDescriptorCollection"/> containing the property descriptions for the object represented by this type descriptor. The default is <see cref="F:System.ComponentModel.PropertyDescriptorCollection.Empty"/>.
        /// </returns>
        public override PropertyDescriptorCollection GetProperties()
        {
            // Enumerate the original set of properties and create our new set with it
            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();

            foreach (ExifProperty prop in owner.Properties)
            {
                ExifPropertyDescriptor pd = new ExifPropertyDescriptor(prop);
                properties.Add(pd);
            }

            // Finally return the list
            return new PropertyDescriptorCollection(properties.ToArray(), true);
        }
    }
    internal sealed class ExifPropertyDescriptor : PropertyDescriptor
    {
        object originalValue;
        ExifProperty linkedProperty;

        public ExifPropertyDescriptor(ExifProperty property)
            : base(property.Name, new Attribute[] { new BrowsableAttribute(true) })
        {
            linkedProperty = property;
            originalValue = property.Value;
        }

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override Type ComponentType
        {
            get { return typeof(JPEGFile); }
        }

        public override object GetValue(object component)
        {
            return linkedProperty.Value;
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type PropertyType
        {
            get { return linkedProperty.Value.GetType(); }
        }

        public override void ResetValue(object component)
        {
            linkedProperty.Value = originalValue;
        }

        public override void SetValue(object component, object value)
        {
            linkedProperty.Value = value;
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}
