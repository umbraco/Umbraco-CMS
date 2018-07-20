using System.Collections.Generic;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// ApiController to provide RTE configuration with available plugins and commands from the RTE config
    /// </summary>
    [PluginController("UmbracoApi")]
    public class RichTextPreValueController : UmbracoAuthorizedJsonController
    {
        private static volatile bool _init;
        private static readonly object Locker = new object();
        private static readonly Dictionary<string, RichTextEditorCommand> Commands = new Dictionary<string, RichTextEditorCommand>();
        private static readonly Dictionary<string, RichTextEditorPlugin> Plugins = new Dictionary<string, RichTextEditorPlugin>();
        private static readonly Dictionary<string, string> ConfigOptions = new Dictionary<string, string>();

        private static string _invalidElements = "";
        private static string _validElements = "";

        public RichTextEditorConfiguration GetConfiguration()
        {
            EnsureInit();

            var config = new RichTextEditorConfiguration
            {
                Plugins = Plugins.Values,
                Commands = Commands.Values,
                ValidElements = _validElements,
                InvalidElements = _invalidElements,
                CustomConfig = ConfigOptions
            };

            return config;
        }

        private static void EnsureInit()
        {

            if (_init == false)
            {
                lock (Locker)
                {
                    if (_init == false)
                    {
                        // Load config
                        XmlDocument xd = new XmlDocument();
                        xd.Load(IOHelper.MapPath(SystemFiles.TinyMceConfig));

                        foreach (XmlNode n in xd.DocumentElement.SelectNodes("//command"))
                        {
                            var alias = n.AttributeValue<string>("alias").ToLower();

                            if (!Commands.ContainsKey(alias))
                                Commands.Add(
                                    alias,
                                    new RichTextEditorCommand()
                                    {
                                        Name = n.AttributeValue<string>("name") ?? alias,
                                        Alias = alias,
                                        Mode = Enum<RichTextEditorCommandMode>.Parse(n.AttributeValue<string>("mode"), true)
                                    }
                                );
                        }


                        foreach (XmlNode n in xd.DocumentElement.SelectNodes("//plugin"))
                        {
                            if (!Plugins.ContainsKey(n.FirstChild.Value))
                            {

                                Plugins.Add(
                                    n.FirstChild.Value.ToLower(),
                                    new RichTextEditorPlugin()
                                    {
                                        Name = n.FirstChild.Value,
                                    });
                            }
                        }


                        foreach (XmlNode n in xd.DocumentElement.SelectNodes("//config"))
                        {
                            if (!ConfigOptions.ContainsKey(n.Attributes["key"].FirstChild.Value))
                            {
                                var value = "";
                                if (n.FirstChild != null)
                                    value = n.FirstChild.Value;

                                ConfigOptions.Add(
                                    n.Attributes["key"].FirstChild.Value.ToLower(),
                                    value);
                            }
                        }

                        if (xd.DocumentElement.SelectSingleNode("./invalidElements") != null)
                            _invalidElements = xd.DocumentElement.SelectSingleNode("./invalidElements").FirstChild.Value;
                        if (xd.DocumentElement.SelectSingleNode("./validElements") != null)
                        {
                            string _val = xd.DocumentElement.SelectSingleNode("./validElements").FirstChild.Value.Replace("\r", "").Replace("\n", "");
                            _validElements = _val;

                            /*foreach (string s in _val.Split("\n".ToCharArray()))
                                _validElements += "'" + s + "' + \n";
                            _validElements = _validElements.Substring(0, _validElements.Length - 4);*/
                        }

                        _init = true;
                    }
                }
            }

        }

    }
}
