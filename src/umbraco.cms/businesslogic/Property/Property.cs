using System;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Web.UI;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.propertytype;
using umbraco.interfaces;

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

        /// <summary>
        /// Unused, please do not use
        /// </summary>
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
            if (_pt.DataTypeDefinition.DataType == null)
                throw new Exception(string.Format("Could not load datatype '{0}'", _pt.DataTypeDefinition.Text));
            _data = _pt.DataTypeDefinition.DataType.Data;
            _data.PropertyId = Id;
        }

        public Property(int Id)
        {
            _id = Id;
            using (var sqlHelper = Application.SqlHelper)
            {
                _pt = PropertyType.GetPropertyType(
                    sqlHelper.ExecuteScalar<int>("select propertytypeid from cmsPropertyData where id = @id",
                        sqlHelper.CreateParameter("@id", Id)));
                if (_pt.DataTypeDefinition.DataType == null)
                    throw new Exception(string.Format("Could not load datatype '{0}'", _pt.DataTypeDefinition.Text));
                _data = _pt.DataTypeDefinition.DataType.Data;
                _data.PropertyId = Id;
            }
        }

        internal Property(Umbraco.Core.Models.Property property)
        {
            _id = property.Id;
            _property = property;
            _propertyType = property.PropertyType;

            //Just to ensure that there is a PropertyType available
            _pt = PropertyType.GetPropertyType(property.PropertyTypeId);

            //ensure we have data property editor set
            if (_pt.DataTypeDefinition.DataType != null)
            {
                _data = _pt.DataTypeDefinition.DataType.Data;
            }
            else
            {
                //send back null we will handle it in ContentControl AddControlNew 
                //and display to use message from the dictionary errors section 
                _data= new DefaultData(null);
            }
            
            _data.PropertyId = Id;

            //set the value so it doesn't need to go to the database
            var dvs = _data as IDataValueSetter;
            if (dvs != null)
            {
                dvs.SetValue(property.Value, property.PropertyType.DataTypeDatabaseType.ToString());
            }
            
        }

        public Guid VersionId
        {
            get
            {
                using (var sqlHelper = Application.SqlHelper)
                using (IRecordsReader dr = sqlHelper.ExecuteReader("SELECT versionId FROM cmsPropertyData WHERE id = " + _id.ToString()))
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
            using (var sqlHelper = Application.SqlHelper)
            {
                int contentId = sqlHelper.ExecuteScalar<int>("Select contentNodeId from cmsPropertyData where Id = " + _id);

                sqlHelper.ExecuteNonQuery("Delete from cmsPropertyData where PropertyTypeId =" + _pt.Id + " And contentNodeId = " + contentId);

                _data.Delete();
            }
        }

        public XmlNode ToXml(XmlDocument xd)
        {
            var serializer = new EntityXmlSerializer();
            var xml = serializer.Serialize(ApplicationContext.Current.Services.DataTypeService, this._property);
            return xml.GetXmlNode(xd);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Property MakeNew(PropertyType pt, Content c, Guid versionId)
        {
            int newPropertyId = 0;
            // The method is synchronized
            using (var sqlHelper = Application.SqlHelper)
            {
                sqlHelper.ExecuteNonQuery(
                    "INSERT INTO cmsPropertyData (contentNodeId, versionId, propertyTypeId) VALUES(@contentNodeId, @versionId, @propertyTypeId)",
                    sqlHelper.CreateParameter("@contentNodeId", c.Id),
                    sqlHelper.CreateParameter("@versionId", versionId),
                    sqlHelper.CreateParameter("@propertyTypeId", pt.Id));
                newPropertyId = sqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM cmsPropertyData");
                interfaces.IData d = pt.DataTypeDefinition.DataType.Data;
                d.MakeNew(newPropertyId);
                return new Property(newPropertyId, pt);
            }
        }
    }
}