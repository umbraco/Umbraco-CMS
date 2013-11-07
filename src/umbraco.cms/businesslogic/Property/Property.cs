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
                SqlHelper.ExecuteScalar<int>("select propertytypeid from cmsPropertyData where id = @id",
                                             SqlHelper.CreateParameter("@id", Id)));
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

            if (_pt.DataTypeDefinition.DataType== null)
                throw new Exception(string.Format("The datatype '{0}' for property '{1} couldn't be loaded", _pt.DataTypeDefinition.Text, _pt.Name));

            _data = _pt.DataTypeDefinition.DataType.Data;
            _data.PropertyId = Id;
        }

        public Guid VersionId
        {
            get
            {
                using (IRecordsReader dr = SqlHelper.ExecuteReader("SELECT versionId FROM cmsPropertyData WHERE id = " + _id.ToString()))
                {
                    dr.Read();
                    return dr.GetGuid("versionId");
                }
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
            int contentId = SqlHelper.ExecuteScalar<int>("Select contentNodeId from cmsPropertyData where Id = " + _id);
            SqlHelper.ExecuteNonQuery("Delete from cmsPropertyData where PropertyTypeId =" + _pt.Id + " And contentNodeId = " + contentId);
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
            SqlHelper.ExecuteNonQuery("INSERT INTO cmsPropertyData (contentNodeId, versionId, propertyTypeId) VALUES(@contentNodeId, @versionId, @propertyTypeId)",
                                      SqlHelper.CreateParameter("@contentNodeId", c.Id),
                                      SqlHelper.CreateParameter("@versionId", versionId),
                                      SqlHelper.CreateParameter("@propertyTypeId", pt.Id));
            newPropertyId = SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM cmsPropertyData");
            interfaces.IData d = pt.DataTypeDefinition.DataType.Data;
            d.MakeNew(newPropertyId);
            return new Property(newPropertyId, pt);
        }
    }

}