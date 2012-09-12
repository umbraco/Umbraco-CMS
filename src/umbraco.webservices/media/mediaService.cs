using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Web.Services;
using Umbraco.Core.Media;
using umbraco.IO;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.property;
using Umbraco.Core.IO;
using System.Xml;
using System.Web;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web.Script.Services;

namespace umbraco.webservices.media
{
    [WebService(Namespace = "http://umbraco.org/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class mediaService : BaseWebService
    {
        internal IMediaFileSystem _fs;

        public mediaService()
        {
            _fs = FileSystemProviderManager.Current.GetFileSystemProvider<IMediaFileSystem>();
        }

        override public Services Service
        {
            get
            {
                return Services.MediaService;
            }
        }

        [WebMethod]
        public void update(mediaCarrier carrier, string username, string password)
        {

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
                    if (!(updatedproperty.Key.ToLower().Equals("umbracofile")))
                    {
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
                    var path = _fs.GetRelativePath(p.Value.ToString());
                    if(_fs.FileExists(path))
                        _fs.DeleteFile(path, true);
                }
            }

            m.delete();

        }

        [WebMethod]
        public void writeContents(int id, string filename, Byte[] contents, string username, string password)
        {
            Authenticate(username, password);

			filename = filename.Replace("/", global::Umbraco.Core.IO.IOHelper.DirSepChar.ToString());
			filename = filename.Replace(@"\", global::Umbraco.Core.IO.IOHelper.DirSepChar.ToString());
			filename = filename.Substring(filename.LastIndexOf(global::Umbraco.Core.IO.IOHelper.DirSepChar) + 1, filename.Length - filename.LastIndexOf(global::Umbraco.Core.IO.IOHelper.DirSepChar) - 1).ToLower();

            Media m = new Media(id);

            var path = _fs.GetRelativePath(m.getProperty("umbracoFile").Id, filename);

            var stream = new MemoryStream();
            stream.Write(contents, 0, contents.Length);
            stream.Seek(0, 0);

            _fs.AddFile(path, stream);

            m.getProperty("umbracoFile").Value = _fs.GetUrl(path);
            m.getProperty("umbracoExtension").Value = Path.GetExtension(filename).Substring(1);
            m.getProperty("umbracoBytes").Value = _fs.GetSize(path);


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

       

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Result Embed(string url, int width, int height)
        {
            Result r = new Result();
            
            //todo cache embed doc
            var xmlConfig = new XmlDocument();
            xmlConfig.Load(GlobalSettings.FullpathToRoot + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "EmbeddedMedia.config");

            foreach (XmlNode node in xmlConfig.SelectNodes("//provider"))
            {
                var regexPattern = new Regex(node.SelectSingleNode("./urlShemeRegex").InnerText, RegexOptions.IgnoreCase);
                if (regexPattern.IsMatch(url))
                {
                    var type = node.Attributes["type"].Value;

                    var prov = (IEmbedProvider)Activator.CreateInstance(Type.GetType(node.Attributes["type"].Value));

                    if (node.Attributes["supportsDimensions"] != null)
                        r.SupportsDimensions = node.Attributes["supportsDimensions"].Value == "True";
                    else
                        r.SupportsDimensions = prov.SupportsDimensions;

                    var settings = node.ChildNodes.Cast<XmlNode>().ToDictionary(settingNode => settingNode.Name);

                    foreach (var prop in prov.GetType().GetProperties().Where(prop => prop.IsDefined(typeof(ProviderSetting), true)))
                    {

                        if (settings.Any(s => s.Key.ToLower() == prop.Name.ToLower()))
                        {
                            XmlNode setting = settings.FirstOrDefault(s => s.Key.ToLower() == prop.Name.ToLower()).Value;
                            var settingType = typeof(Umbraco.Web.Media.EmbedProviders.Settings.String);

                            if (setting.Attributes["type"] != null)
                                settingType = Type.GetType(setting.Attributes["type"].Value);

                            var settingProv = (IEmbedSettingProvider)Activator.CreateInstance(settingType);
                            prop.SetValue(prov, settingProv.GetSetting(settings.FirstOrDefault(s => s.Key.ToLower() == prop.Name.ToLower()).Value), null);

                        }


                    }
                    try
                    {
                        r.Markup = prov.GetMarkup(url, width, height);
                        r.Status = Status.Success;
                    }
                    catch
                    {
                        r.Status = Status.Error;
                    }
                    return r;
                }
            }

            r.Status = Status.NotSupported;
            return r;
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

            foreach (Property p in m.getProperties)
            {

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
}