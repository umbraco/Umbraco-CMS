using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
        private static bool _init = false;
        private static Dictionary<string, RichTextEditorCommand> _commands = new Dictionary<string,RichTextEditorCommand>();
        private static Dictionary<string, RichTextEditorPlugin> _plugins = new Dictionary<string, RichTextEditorPlugin>();
        private static Dictionary<string, string> _configOptions = new Dictionary<string, string>();
       
        private static string _invalidElements = "";
        private static string _validElements = "";


        public RichTextEditorConfiguration GetConfiguration()
        {
            if (!_init)
                init();


            RichTextEditorConfiguration config = new RichTextEditorConfiguration();
            config.Plugins = _plugins.Values;
            config.Commands = _commands.Values;
            config.ValidElements = _validElements;
            config.InvalidElements = _invalidElements;
            config.CustomConfig = _configOptions;

            return config;
        }



        private static void init()
        {
            // Load config
            XmlDocument xd = new XmlDocument();
            xd.Load(IOHelper.MapPath(SystemFiles.TinyMceConfig));

            foreach (XmlNode n in xd.DocumentElement.SelectNodes("//command"))
            {   
                var alias = n.SelectSingleNode("./umbracoAlias").FirstChild.Value.ToLower();

                bool isStyle = false;
                if (n.Attributes.GetNamedItem("isStyle") != null)
                    isStyle = bool.Parse(n.Attributes.GetNamedItem("isStyle").Value);
                    
                if(!_commands.ContainsKey(alias))
                    _commands.Add(
                             alias,
                             new RichTextEditorCommand()
                             {
                                 IsStylePicker = isStyle,
                                 Icon = n.SelectSingleNode("./icon").FirstChild.Value,
                                 Command = n.SelectSingleNode("./tinyMceCommand").FirstChild.Value,
                                 Alias = n.SelectSingleNode("./umbracoAlias").FirstChild.Value.ToLower(),
                                 UserInterface = n.SelectSingleNode("./tinyMceCommand").Attributes.GetNamedItem("userInterface").Value,
                                 FrontEndCommand = n.SelectSingleNode("./tinyMceCommand").Attributes.GetNamedItem("frontendCommand").Value,
                                 Value = n.SelectSingleNode("./tinyMceCommand").Attributes.GetNamedItem("value").Value,
                                 Priority = int.Parse(n.SelectSingleNode("./priority").FirstChild.Value)
                             }
                       );
            }


            foreach (XmlNode n in xd.DocumentElement.SelectNodes("//plugin"))
            {
                if (!_plugins.ContainsKey(n.FirstChild.Value))
                {
                    bool useOnFrontend = false;
                    if (n.Attributes.GetNamedItem("loadOnFrontend") != null)
                        useOnFrontend = bool.Parse(n.Attributes.GetNamedItem("loadOnFrontend").Value);

                    _plugins.Add(
                        n.FirstChild.Value.ToLower(),
                        new RichTextEditorPlugin(){
                            Name = n.FirstChild.Value,
                            UseOnFrontend = useOnFrontend});
                }
            }


            foreach (XmlNode n in xd.DocumentElement.SelectNodes("//config"))
            {
                if (!_configOptions.ContainsKey(n.Attributes["key"].FirstChild.Value))
                {
                    var value = "";
                    if (n.FirstChild != null)
                        value = n.FirstChild.Value;

                    _configOptions.Add(
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
