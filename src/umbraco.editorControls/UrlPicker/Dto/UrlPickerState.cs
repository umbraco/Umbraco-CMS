using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace umbraco.editorControls.UrlPicker.Dto
{
    using umbraco;

    using Umbraco.Core.Services;
    using Umbraco.Web;

    /// <summary>
    /// The DTO which contains the state of a URL picker at any time.
    /// </summary>
    [Serializable]
    public class UrlPickerState
    {
        #region DTO Properties

        /// <summary>
        /// Title for the URL
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Mode that the URL picker is set to.  See UrlPickerMode.
        /// </summary>
        public UrlPickerMode Mode { get; set; }
        /// <summary>
        /// Node ID, if set, for a content node
        /// </summary>
        public int? NodeId { get; set; }
        /// <summary>
        /// URL which is the whole point of this datatype
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Whether the URL is to open in a new window
        /// </summary>
        public bool NewWindow { get; set; }

        #endregion

        /// <summary>
        /// Set defaults
        /// </summary>
        public UrlPickerState()
        {
            Mode = UrlPickerMode.URL;
            NewWindow = false;
        }

        /// <summary>
        /// Set defaults as defined by a UrlPickerSettings
        /// </summary>
        /// <param name="settings"></param>
        public UrlPickerState(UrlPickerSettings settings)
            : base()
        {
            Mode = settings.DefaultMode;

            if (!settings.AllowedModes.Contains(Mode))
            {
                Mode = settings.AllowedModes.First();
            }
        }

        /// <summary>
        /// Validates the state as far as the state can validate itself
        /// </summary>
        /// <returns>True if the state validates</returns>
        public bool Validates()
        {
            // The mandatory URL is not specified
            // EDIT: URL is not mandatory anymore (due to popular demand)
            //if (string.IsNullOrEmpty(Url))
            //{
            //    return false;
            //}

            // The URL is incorrect (failed Ajax call for content node)
            // EDIT: was causing problems when not bothering to pick a content node
            //if (Mode == UrlPickerMode.Content)
            //{
            //    if (!NodeId.HasValue || Url != umbraco.library.NiceUrl(NodeId.GetValueOrDefault()))
            //    {
            //        return false;
            //    }
            //}
            //}

            return true;
        }

        /// <summary>
        /// Serializes this state into a number of formats
        /// </summary>
        /// <param name="format">The format desired</param>
        /// <returns>A serialized string</returns>
        public string Serialize(UrlPickerDataFormat format)
        {             
            // Serialized return string
            string serializedData;

            // Get text-strings (for XML/CSV)
            string mode = this.Mode.ToString();
            string newWindow = this.NewWindow.ToString();
            string nodeId = ((!this.NodeId.HasValue)
                 ? ""
                 : this.NodeId.GetValueOrDefault().ToString());
            string linkTitle = string.IsNullOrEmpty(this.Title)
                ? ""
                : this.Title;
            string url = string.IsNullOrEmpty(this.Url)
                ? ""
                : this.Url; ;

            switch (format)
            {
                case UrlPickerDataFormat.Xml:

                    serializedData = new XElement("url-picker",
                                        new XAttribute("mode", mode),
                                        new XElement("new-window", newWindow),
                                        new XElement("node-id", nodeId),
                                        new XElement("url", url),
                                        new XElement("link-title", linkTitle)
                                    ).ToString();

                    break;

                case UrlPickerDataFormat.Csv:
                    // Making sure to escape commas:
                    serializedData = mode + "," +
                                        newWindow + "," +
                                        nodeId + "," +
                                        url.Replace(",", "&#45;") + "," +
                                        linkTitle.Replace(",", "&#45;");

                    break;

                case UrlPickerDataFormat.Json:

                    var jss = new JavaScriptSerializer();
                    serializedData = jss.Serialize(this);

                    break;

                default:
                    throw new NotImplementedException();
            }

            // Return empty if no serialized data set
            return serializedData;
        }

        /// <summary>
        /// Returns a UrlPickerState based on a serialized string.
        /// 
        /// Tries to infer the format of the serialized data based on known types.  Will throw exceptions
        /// if it fails to parse.
        /// </summary>
        /// <param name="serializedState">An instance of UrlPickerState as a serialized string</param>
        /// <returns>The state</returns>
        public static UrlPickerState Deserialize(string serializedState)
        {
            // Can't deserialize an empty whatever
            if (string.IsNullOrEmpty(serializedState))
            {
                return null;
            }

            // Default
            var state = new UrlPickerState();

            // Imply data format from the formatting of the serialized state
            UrlPickerDataFormat impliedDataFormat;
            if (serializedState.StartsWith("<"))
            {
                impliedDataFormat = UrlPickerDataFormat.Xml;
            }
            else if (serializedState.StartsWith("{"))
            {
                impliedDataFormat = UrlPickerDataFormat.Json;
            }
            else
            {
                impliedDataFormat = UrlPickerDataFormat.Csv;
            }

            // Try to deserialize the string
            try
            {
                switch (impliedDataFormat)
                {
                    case UrlPickerDataFormat.Xml:
                        var dataNode = XElement.Parse(serializedState);

                        // Carefully try to get values out.  This is in case new versions add
                        // to the XML
                        var modeAttribute = dataNode.Attribute("mode");
                        if (modeAttribute != null)
                        {
                            state.Mode = (UrlPickerMode)Enum.Parse(typeof(UrlPickerMode), modeAttribute.Value, false);
                        }

                        var newWindowElement = dataNode.Element("new-window");
                        if (newWindowElement != null)
                        {
                            state.NewWindow = bool.Parse(newWindowElement.Value);
                        }

                        var nodeIdElement = dataNode.Element("node-id");
                        if (nodeIdElement != null)
                        {
                            int nodeId;
                            if (int.TryParse(nodeIdElement.Value, out nodeId))
                            {
                                state.NodeId = nodeId;
                            }
                        }

                        var urlElement = dataNode.Element("url");
                        if (urlElement != null)
                        {
                            state.Url = urlElement.Value;
                        }

                        var linkTitleElement = dataNode.Element("link-title");
                        if (linkTitleElement != null && !string.IsNullOrEmpty(linkTitleElement.Value))
                        {
                            state.Title = linkTitleElement.Value;
                        }

                        break;
                    case UrlPickerDataFormat.Csv:

                        var parameters = serializedState.Split(',');

                        if (parameters.Length > 0)
                        {
                            state.Mode = (UrlPickerMode)Enum.Parse(typeof(UrlPickerMode), parameters[0], false);
                        }
                        if (parameters.Length > 1)
                        {
                            state.NewWindow = bool.Parse(parameters[1]);
                        }
                        if (parameters.Length > 2)
                        {
                            int nodeId;
                            if (int.TryParse(parameters[2], out nodeId))
                            {
                                state.NodeId = nodeId;
                            }
                        }
                        if (parameters.Length > 3)
                        {
                            state.Url = parameters[3].Replace("&#45;", ",");
                        }
                        if (parameters.Length > 4)
                        {
                            if (!string.IsNullOrEmpty(parameters[4]))
                            {
                                state.Title = parameters[4].Replace("&#45;", ",");
                            }
                        }

                        break;
                    case UrlPickerDataFormat.Json:

                        var jss = new JavaScriptSerializer();
                        state = jss.Deserialize<UrlPickerState>(serializedState);

                        // Check for old states
                        var untypedState = jss.DeserializeObject(serializedState);
                        if (untypedState is Dictionary<string, object>)
                        {
                            var dictState = (Dictionary<string, object>)untypedState;

                            if (dictState.ContainsKey("LinkTitle"))
                            {
                                state.Title = (string)dictState["LinkTitle"];

                                if (dictState.ContainsKey("NewWindow"))
                                {
                                    // There was a short period where the UrlPickerMode values were
                                    // integers starting with zero, instead of one.  This period only
                                    // existed when both the "LinkTitle" and "NewWindow" keys were
                                    // used.
                                    state.Mode = (UrlPickerMode)((int)dictState["Mode"] + 1);
                                }
                            }
                        }

                        break;
                    default:
                        throw new NotImplementedException();
                }
                
                 // If the mode is a content node, get the url for the node
                 if (state.Mode == UrlPickerMode.Content && state.NodeId.HasValue && UmbracoContext.Current != null)
                 {
                     var n = uQuery.GetNode(state.NodeId.Value);
                     var url = n != null ? n.Url : "#";
  
                     if (!string.IsNullOrWhiteSpace(url))
                     {
                         state.Url = url;
                     }

                     if (string.IsNullOrWhiteSpace(state.Title) && n != null)
                     {
                         state.Title = n.Name;
                     }
                 }
            }
            catch (Exception)
            {
                // Could not be deserialised, return null
                state = null;
            }

            return state;
        }
    }
}
