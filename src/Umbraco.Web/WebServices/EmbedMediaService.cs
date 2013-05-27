using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.Media;
using umbraco.BusinessLogic;
using Umbraco.Web.BaseRest;

namespace Umbraco.Web.WebServices
{
    [RestExtension("EmbedMediaService")]
    public class EmbedMediaService
    {
        [RestExtensionMethod(ReturnXml = false)]
        public static string Embed()
        {
            var currentUser = User.GetCurrent();

            if (currentUser == null)
                throw new UnauthorizedAccessException("You must be logged in to use this service");
            
            var url = HttpContext.Current.Request.Form["url"];
            var width = int.Parse(HttpContext.Current.Request.Form["width"]);
            var height = int.Parse(HttpContext.Current.Request.Form["height"]);

            var result = new Result();

            //todo cache embed doc
            var xmlConfig = new XmlDocument();
            xmlConfig.Load(GlobalSettings.FullpathToRoot + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "EmbeddedMedia.config");

            foreach (XmlNode node in xmlConfig.SelectNodes("//provider"))
            {
                var regexPattern = new Regex(node.SelectSingleNode("./urlShemeRegex").InnerText, RegexOptions.IgnoreCase);

                if (regexPattern.IsMatch(url))
                {
                    var prov = (IEmbedProvider)Activator.CreateInstance(Type.GetType(node.Attributes["type"].Value));

                    if (node.Attributes["supportsDimensions"] != null)
                        result.SupportsDimensions = node.Attributes["supportsDimensions"].Value == "True";
                    else
                        result.SupportsDimensions = prov.SupportsDimensions;

                    var settings = node.ChildNodes.Cast<XmlNode>().ToDictionary(settingNode => settingNode.Name);

                    foreach (var prop in prov.GetType().GetProperties().Where(prop => prop.IsDefined(typeof(ProviderSetting), true)))
                    {

                        if (settings.Any(s => s.Key.ToLower() == prop.Name.ToLower()))
                        {
                            var setting = settings.FirstOrDefault(s => s.Key.ToLower() == prop.Name.ToLower()).Value;
                            var settingType = typeof(Media.EmbedProviders.Settings.String);

                            if (setting.Attributes["type"] != null)
                                settingType = Type.GetType(setting.Attributes["type"].Value);

                            var settingProv = (IEmbedSettingProvider)Activator.CreateInstance(settingType);
                            prop.SetValue(prov, settingProv.GetSetting(settings.FirstOrDefault(s => s.Key.ToLower() == prop.Name.ToLower()).Value), null);
                        }
                    }
                    try
                    {
                        result.Markup = prov.GetMarkup(url, width, height);
                        result.Status = Status.Success;
                    }
                    catch
                    {
                        result.Status = Status.Error;
                    }

                    return new JavaScriptSerializer().Serialize(result);
                }
            }

            result.Status = Status.NotSupported;
            return new JavaScriptSerializer().Serialize(result);
        }
    }
}
