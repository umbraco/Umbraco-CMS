using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using Umbraco.Core.IO;

namespace umbraco.editorControls.tinymce
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class tinyMCEConfiguration
    {
        private static bool _init = false;

        private static Hashtable _commands = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        private static string _validElements;

        private static Hashtable _configOptions = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        public static Hashtable ConfigOptions
        {
            get
            {
                if (!_init)
                    init();
                return _configOptions;
            }
            set
            {
                _configOptions = value;
            }
        }

        public static string ValidElements
        {
            get
            {
                if (!_init)
                    init();
                return _validElements;
            }
            set { _validElements = value; }
        }

        public static string PluginPath = IOHelper.ResolveUrl( SystemDirectories.Umbraco ) + "/plugins/tinymce3";
        public static string JavascriptPath = IOHelper.ResolveUrl( SystemDirectories.UmbracoClient ) + "/tinymce3";

        private static string _invalidElements;

        public static string InvalidElements
        {
            get
            {
                if (!_init)
                    init();
                return _invalidElements;
            }
            set { _invalidElements = value; }
        }

        private static Hashtable _plugins = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        public static Hashtable Plugins
        {
            get
            {
                if (!_init)
                    init();
                return _plugins;
            }
            set { _plugins = value; }
        }



        public static Hashtable Commands
        {
            get
            {
                if (!_init)
                    init();
                return _commands;
            }
        }

        public static SortedList SortedCommands
        {
            get
            {
                if (!_init)
                    init();

                SortedList sc = new SortedList();
                IDictionaryEnumerator ide = _commands.GetEnumerator();
                while (ide.MoveNext())
                    sc.Add(((tinyMCECommand)ide.Value).Priority, (tinyMCECommand)ide.Value);
                return sc;
            }
        }

        private static void init()
        {
            // Load config
            XmlDocument xd = new XmlDocument();
            xd.Load(IOHelper.MapPath( SystemFiles.TinyMceConfig ));

            foreach (XmlNode n in xd.DocumentElement.SelectNodes("//command"))
            {
                if (!_commands.ContainsKey(n.SelectSingleNode("./umbracoAlias").FirstChild.Value))
                {
                    bool isStyle = false;
                    if (n.Attributes.GetNamedItem("isStyle") != null)
                        isStyle = bool.Parse(n.Attributes.GetNamedItem("isStyle").Value);

                    _commands.Add(
                        n.SelectSingleNode("./umbracoAlias").FirstChild.Value.ToLower(),
                        new tinyMCECommand(
                            isStyle,
                            n.SelectSingleNode("./icon").FirstChild.Value,
                            n.SelectSingleNode("./tinyMceCommand").FirstChild.Value,
                            n.SelectSingleNode("./umbracoAlias").FirstChild.Value.ToLower(),
                            n.SelectSingleNode("./tinyMceCommand").Attributes.GetNamedItem("userInterface").Value,
                            n.SelectSingleNode("./tinyMceCommand").Attributes.GetNamedItem("frontendCommand").Value,
                            n.SelectSingleNode("./tinyMceCommand").Attributes.GetNamedItem("value").Value,
                            int.Parse(n.SelectSingleNode("./priority").FirstChild.Value)
                            ));
                }
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
                        new tinyMCEPlugin(
                            n.FirstChild.Value,
                            useOnFrontend));
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
                string _val = xd.DocumentElement.SelectSingleNode("./validElements").FirstChild.Value.Replace("\r", "");
                foreach (string s in _val.Split("\n".ToCharArray()))
                    _validElements += "'" + s + "' + \n";
                _validElements = _validElements.Substring(0, _validElements.Length - 4);

            }

            _init = true;
        }
    }

    public class tinyMCEPlugin
    {
        public tinyMCEPlugin(string Name, bool UseOnFrontEnd)
        {
            _name = Name;
            _useOnFrontend = UseOnFrontEnd;
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }


        private bool _useOnFrontend;

        public bool UseOnFrontend
        {
            get { return _useOnFrontend; }
            set { _useOnFrontend = value; }
        }

    }

    public class tinyMCECommand
    {

        public tinyMCECommand(bool isStylePicker, string Icon, string Command, string Alias, string UserInterface, string FrontEndCommand, string Value, int Priority)
        {
            _isStylePicker = isStylePicker;
            _icon = Icon;
            _command = Command;
            _alias = Alias;
            _userInterface = UserInterface;
            _frontEndCommand = FrontEndCommand;
            _value = Value;
            _priority = Priority;
        }

        private bool _isStylePicker;

        public bool IsStylePicker
        {
            get { return _isStylePicker; }
            set { _isStylePicker = value; }
        }


        private string _icon;

        public string Icon
        {
            get { return IOHelper.ResolveUrl( SystemDirectories.Umbraco ) + "/" + _icon; }
            set { _icon = value; }
        }

        private string _command;

        public string Command
        {
            get { return _command; }
            set { _command = value; }
        }

        private string _alias;

        public string Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }

        private string _userInterface;

        public string UserInterface
        {
            get { return _userInterface; }
            set { _userInterface = value; }
        }

        private string _frontEndCommand;

        public string FrontEndCommand
        {
            get { return _frontEndCommand; }
            set { _frontEndCommand = value; }
        }

        private string _value;

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private int _priority;

        public int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }








    }
}
