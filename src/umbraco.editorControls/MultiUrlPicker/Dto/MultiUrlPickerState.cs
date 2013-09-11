using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using umbraco.editorControls.UrlPicker;
using umbraco.editorControls.UrlPicker.Dto;

namespace umbraco.editorControls.MultiUrlPicker.Dto
{
    /// <summary>
    /// The DTO which contains the state of a Multi URL picker at any time.
    /// </summary>
    [Serializable]
    public class MultiUrlPickerState
    {
        /// <summary>
        /// The items created
        /// </summary>
        public IEnumerable<UrlPickerState> Items { get; set; }

        /// <summary>
        /// Sets defaults
        /// </summary>
        public MultiUrlPickerState()
        {
            Items = new List<UrlPickerState>();
        }

        /// <summary>
        /// Validates the state as far as the state can validate itself
        /// </summary>
        /// <returns>True if the state validates</returns>
        public bool Validates()
        {
            // return false if any item doesn't validate
            return !Items.Any(x => !x.Validates());
        }

        /// <summary>
        /// Serializes this state into a number of formats
        /// </summary>
        /// <param name="format">The format desired</param>
        /// <param name="includeEmptyItems">if set to <c>true</c> [include empty items].</param>
        /// <returns>A serialized string</returns>
        public string Serialize(UrlPickerDataFormat format, bool includeEmptyItems = true)
        {
            // Serialized return string
            string serializedData = string.Empty;

            switch (format)
            {
                case UrlPickerDataFormat.Xml:

                    var xmlElement = new XElement("multi-url-picker");

                    foreach (var item in Items)
                    {
                        if (includeEmptyItems || !string.IsNullOrEmpty(item.Url))
                        {
                            var serializedItem = item.Serialize(UrlPickerDataFormat.Xml);
                            if (!string.IsNullOrEmpty(serializedItem))
                            {
                                xmlElement.Add(XElement.Parse(serializedItem));
                            }
                        }
                    }

                    serializedData = xmlElement.ToString();

                    break;

                case UrlPickerDataFormat.Csv:

                    foreach (var item in Items)
                    {
                        if (includeEmptyItems || !string.IsNullOrEmpty(item.Url))
                        {
                            var serializedItem = item.Serialize(UrlPickerDataFormat.Csv);

                            if (!string.IsNullOrEmpty(serializedItem))
                            {
                                serializedData += serializedItem + "\n";
                            }
                        }
                    }

                    break;

                case UrlPickerDataFormat.Json:

                    if (!includeEmptyItems)
                    {
                        // Bit of a cheat to remove empty items - serializes and deserializes to clone the object
                        var clone = MultiUrlPickerState.Deserialize(this.Serialize(UrlPickerDataFormat.Xml, true));

                        // Remove empty items
                        clone.Items = clone.Items.Where(x => !string.IsNullOrEmpty(x.Url));

                        var jss = new JavaScriptSerializer();
                        serializedData = jss.Serialize(clone);
                    }
                    else
                    {
                        var jss = new JavaScriptSerializer();
                        serializedData = jss.Serialize(this);
                    }

                    break;

                default:
                    throw new NotImplementedException();
            }

            return serializedData;
        }

        /// <summary>
        /// Returns a MultiUrlPickerState based on a serialized string.
        /// 
        /// Tries to infer the format of the serialized data based on known types.  Will throw exceptions
        /// if it fails to parse.
        /// </summary>
        /// <param name="serializedState">An instance of MultiUrlPickerState as a serialized string</param>
        /// <returns>The state</returns>
        public static MultiUrlPickerState Deserialize(string serializedState)
        {
            // Can't deserialize an empty whatever
            if (string.IsNullOrEmpty(serializedState))
            {
                return null;
            }

            // Default
            var state = new MultiUrlPickerState();
            var items = new List<UrlPickerState>();

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

                        // Get each url-picker node
                        var dataNode = XElement.Parse(serializedState);
                        var xmlItems = dataNode.Elements("url-picker");

                        foreach (var xmlItem in xmlItems)
                        {
                            // Deserialize it
                            items.Add(UrlPickerState.Deserialize(xmlItem.ToString()));
                        }

                        state.Items = items;

                        break;
                    case UrlPickerDataFormat.Csv:

                        // Split CSV by lines
                        var csvItems = serializedState.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                        // Deserialize each line
                        foreach (var csvItem in csvItems)
                        {
                            items.Add(UrlPickerState.Deserialize(csvItem));
                        }

                        state.Items = items;

                        break;
                    case UrlPickerDataFormat.Json:

                        var jss = new JavaScriptSerializer();
                        state = jss.Deserialize<MultiUrlPickerState>(serializedState);

                        break;
                    default:
                        throw new NotImplementedException();
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
