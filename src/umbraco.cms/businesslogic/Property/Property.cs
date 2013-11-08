using System;
using System.Runtime.CompilerServices;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.propertytype;

namespace umbraco.cms.businesslogic.property
{
    /// <summary>
    /// Property class encapsulates property factory, ensuring that the work
    /// with umbraco generic properties stays nice and easy..
    /// </summary>
    public class Property
    {
        private Umbraco.Core.Models.PropertyType _propertyType;
        private Umbraco.Core.Models.Property _property;
        private PropertyType _pt;
        private interfaces.IData _data;
        private int _id;

        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        internal static Database Database
        {
            get { return ApplicationContext.Current.DatabaseContext.Database; }
        }

        public Property(int Id, propertytype.PropertyType pt)
        {

            _pt = pt;
            _id = Id;
            _data = _pt.DataTypeDefinition.DataType.Data;
            _data.PropertyId = Id;
        }

        public Property(int Id)
        {
            _id = Id;

            _pt = PropertyType.GetPropertyType(
                Database.ExecuteScalar<int>("select propertytypeid from cmsPropertyData where id = @0", Id));
            _data = _pt.DataTypeDefinition.DataType.Data;
            _data.PropertyId = Id;
        }

        internal Property(Umbraco.Core.Models.Property property)
        {
            _id = property.Id;
            _property = property;
            _propertyType = property.PropertyType;

            //Just to ensure that there is a PropertyType available
            _pt = PropertyType.GetPropertyType(property.PropertyTypeId);
            _data = _pt.DataTypeDefinition.DataType.Data;
            _data.PropertyId = Id;
        }

        public Guid VersionId
        {
            get
            {
                return Database.ExecuteScalar<Guid>("SELECT versionId FROM cmsPropertyData WHERE id = @0", _id);  
            }
        }
        public int Id
        {
            get { return _id; }
        }
        public propertytype.PropertyType PropertyType
        {
            get
            {
                /*if (_propertyType != null)
                    return new PropertyType(_propertyType);*/

                return _pt;
            }
        }

        public object Value
        {
            get
            {
                return _data.Value;
            }
            set
            {
                _data.Value = value;
            }
        }

        public void delete()
        {
            int contentId = Database.ExecuteScalar<int>("Select contentNodeId from cmsPropertyData where Id = @0", _id);
            Database.Execute("Delete from cmsPropertyData where PropertyTypeId =@0 And contentNodeId = @1", _pt.Id, contentId);
            _data.Delete();
        }
        public XmlNode ToXml(XmlDocument xd)
        {
            string nodeName = UmbracoSettings.UseLegacyXmlSchema ? "data" : helpers.Casing.SafeAlias(PropertyType.Alias);
            XmlNode x = xd.CreateNode(XmlNodeType.Element, nodeName, "");

            // Alias
            if (UmbracoSettings.UseLegacyXmlSchema)
            {
                XmlAttribute alias = xd.CreateAttribute("alias");
                alias.Value = this.PropertyType.Alias;
                x.Attributes.Append(alias);
            }

            x.AppendChild(_data.ToXMl(xd));

            return x;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Property MakeNew(propertytype.PropertyType pt, Content c, Guid versionId)
        {
            int newPropertyId = 0;
            // The method is synchronized
            // UmbracoPropertyDto is used for another entity type - use plain sql for now
            Database.Execute("INSERT INTO cmsPropertyData (contentNodeId, versionId, propertyTypeId) VALUES(@0, @1, @2)", c.Id, versionId, pt.Id); 
            newPropertyId = Database.ExecuteScalar<int>("SELECT MAX(id) FROM cmsPropertyData");
            interfaces.IData d = pt.DataTypeDefinition.DataType.Data;
            d.MakeNew(newPropertyId);
            return new Property(newPropertyId, pt);
        }
    }

}