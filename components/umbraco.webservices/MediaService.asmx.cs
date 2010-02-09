using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using umbraco.cms.businesslogic.web;
using umbraco.cms;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.media;
using System.IO;
using umbraco.cms.businesslogic.property;
using umbraco.IO;
using System.Text.RegularExpressions;

namespace umbraco.webservices.media
{

    [WebService(Namespace = "http://umbraco.org/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class mediaService : BaseWebService
    {

        override public Services Service
        {
            get
            {
                return Services.MediaService;
            }
        }

        [WebMethod]
        public void update(mediaCarrier carrier, string username, string password) {
            
            Authenticate(username, password);
            
            if (carrier == null) throw new Exception("No carrier specified");
            
            Media m = new Media(carrier.Id);
            
            if (carrier.MediaProperties != null)
            {
                foreach (mediaProperty updatedproperty in carrier.MediaProperties)
                {
                    if (!(updatedproperty.Key.ToLower().Equals("umbracofile")))
                    {
                        Property property = m.getProperty(updatedproperty.Key);
                        if (property == null)
                            throw new Exception("property " + updatedproperty.Key + " was not found");
                        property.Value = updatedproperty.PropertyValue;
                    }
                }
            }

            m.Save();
        }

        [WebMethod]
        public int create(mediaCarrier carrier, string username, string password)
        {

            Authenticate(username, password);

            if (carrier == null) throw new Exception("No carrier specified");
            if (carrier.ParentId == 0) throw new Exception("Media needs a parent");
            if (carrier.TypeId == 0) throw new Exception("Type must be specified");
            if (carrier.Text == null || carrier.Text.Length == 0) carrier.Text = "unnamed";
            
            umbraco.BusinessLogic.User user = GetUser(username, password);


            MediaType mt = new MediaType(carrier.TypeId);

            Media m = Media.MakeNew(carrier.Text, mt, user, carrier.ParentId);

            if (carrier.MediaProperties != null)
            {
                foreach (mediaProperty updatedproperty in carrier.MediaProperties)
                {
                    if(!(updatedproperty.Key.ToLower().Equals("umbracofile"))) {
                        Property property = m.getProperty(updatedproperty.Key);
                        if (property == null) 
                            throw new Exception("property " + updatedproperty.Key + " was not found");
                        property.Value = updatedproperty.PropertyValue;
                    }
                }
            }

            return m.Id;
        }

        [WebMethod]
        public void delete(int id, string username, string password)
        {
            Authenticate(username, password);
            
            Media m = new Media(id);

            if (m.HasChildren)
                throw new Exception("Cannot delete Media " + id + " as it has child nodes");

            Property p = m.getProperty("umbracoFile");
            if (p != null)
            {
                if (!(p.Value == System.DBNull.Value))
                {
                    string fileName = (string)p.Value;
                    string filePath = umbraco.IO.IOHelper.MapPath(fileName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }

            m.delete();
            
        }

        [WebMethod]
        public void writeContents(int id, string filename, Byte[] contents, string username, string password)
        {
            Authenticate(username, password);

            filename = filename.Replace("/", IOHelper.DirSepChar.ToString());
            filename = filename.Replace(@"\", IOHelper.DirSepChar.ToString());
            filename = filename.Substring(filename.LastIndexOf(IOHelper.DirSepChar) + 1, filename.Length - filename.LastIndexOf(IOHelper.DirSepChar) - 1).ToLower();
            
            Media m = new Media(id);


            System.IO.Directory.CreateDirectory(IOHelper.MapPath(SystemDirectories.Media + "/" + m.getProperty("umbracoFile").Id ));
            string fullFilePath = IOHelper.MapPath(SystemDirectories.Media + "/" + m.getProperty("umbracoFile").Id + "/" + filename);

            File.WriteAllBytes(fullFilePath, contents);
            
            FileInfo f = new FileInfo(fullFilePath);
            m.getProperty("umbracoFile").Value = SystemDirectories.Media + "/" + filename;
            m.getProperty("umbracoExtension").Value = f.Extension.Replace(".","");
            m.getProperty("umbracoBytes").Value = f.Length.ToString();


        }

        [WebMethod]
        public mediaCarrier read(int id, string username, string password)
        {
            Authenticate(username, password);

            
            Media m = new Media(id);
            
            
            return createCarrier(m);
        }

        [WebMethod]
        public List<mediaCarrier> readList(int parentId, string username, string password)
        {
            Authenticate(username, password);

            List<mediaCarrier> carriers = new List<mediaCarrier>();
            Media[] mediaList;

            if (parentId < 1)
            {
                mediaList = Media.GetRootMedias();
            }
            else
            {
                Media m = new Media(parentId);
                mediaList = m.Children;
            }

            foreach (Media child in mediaList)
            {
                carriers.Add(createCarrier(child));
            }

            return carriers;
        }
        
        private mediaCarrier createCarrier(Media m)
        {
            mediaCarrier carrier = new mediaCarrier();
            carrier.Id = m.Id;
            carrier.Text = m.Text;

            carrier.TypeAlias = m.ContentType.Alias;
            carrier.TypeId = m.ContentType.Id;

            carrier.CreateDateTime = m.CreateDateTime;
            carrier.HasChildren = m.HasChildren;
            carrier.Level = m.Level;

            carrier.Path = m.Path;
            carrier.SortOrder = m.sortOrder;

            try
            {
                carrier.ParentId = m.Parent.Id;
            }
            catch
            {
                carrier.ParentId = -1;
            }

            foreach(Property p in m.getProperties) {
                
                mediaProperty carrierprop = new mediaProperty();

                if (p.Value == System.DBNull.Value)
                {
                    carrierprop.PropertyValue = "";
                }
                else
                {
                    carrierprop.PropertyValue = p.Value;
                }

                carrierprop.Key = p.PropertyType.Alias;
                carrier.MediaProperties.Add(carrierprop);

            }

            return carrier;
        }

    }
    
    

    [Serializable]
    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    public class mediaCarrier
    {
        
        public int Id
        {
            get;
            set;
        }
        public string Text
        {
            get;
            set;

        }

        public string TypeAlias
        {
            get;
            set;
        }

        public int TypeId
        {
            get;
            set;
        }

        public DateTime CreateDateTime
        {
            get;
            set;
        }

        public Boolean HasChildren
        {
            get;
            set;
        }

        public int Level
        {
            get;
            set;
        }

        public int ParentId
        {
            get;
            set;

        }

        public string Path
        {
            get;
            set;
        }

        public int SortOrder
        {
            get;
            set;
        }

        public List<mediaProperty> MediaProperties
        {
            get;
            set;
        }

        public mediaCarrier()
        {
            MediaProperties = new List<mediaProperty>();
        }
    }

    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    public class mediaProperty
    {
        public mediaProperty()
        {
        }

        public object PropertyValue
        {
            get;
            set;
        }

        public string Key
        {
            get;
            set;
        }
    }


}
