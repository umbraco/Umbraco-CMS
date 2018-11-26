using System;
using System.Linq;
using System.Text;
using Umbraco.Core.Logging;
using Examine;
using Lucene.Net.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Xml;
using Umbraco.Examine;

namespace Umbraco.Web.PropertyEditors
{
    using Examine = global::Examine;

    /// <summary>
    /// Represents a grid property and parameter editor.
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.Grid, "Grid layout", "grid", HideLabel = true, ValueType = ValueTypes.Json, Group="rich content", Icon="icon-layout")]
    public class GridPropertyEditor : DataEditor
    {
        public GridPropertyEditor(ILogger logger)
            : base(logger)
        { }

        //TODO: Change this to use a native way of indexing data: https://github.com/umbraco/Umbraco-CMS/issues/3531
        internal void DocumentWriting(object sender, Examine.LuceneEngine.DocumentWritingEventArgs e)
        {
            foreach (var value in e.ValueSet.Values)
            {
                //if there is a value, it's a string and it's detected as json
                if (value.Value.Count > 0 && value.Value[0] != null && (value.Value[0] is string firstVal) && firstVal.DetectIsJson())
                {
                    try
                    {
                        //TODO: We should deserialize this to Umbraco.Core.Models.GridValue instead of doing the below
                        var json = JsonConvert.DeserializeObject<JObject>(firstVal);

                        //check if this is formatted for grid json
                        if (json.HasValues && json.TryGetValue("name", out _) && json.TryGetValue("sections", out _))
                        {
                            //get all values and put them into a single field (using JsonPath)
                            var sb = new StringBuilder();
                            foreach (var row in json.SelectTokens("$.sections[*].rows[*]"))
                            {
                                var rowName = row["name"].Value<string>();
                                var areaVals = row.SelectTokens("$.areas[*].controls[*].value");

                                foreach (var areaVal in areaVals)
                                {
                                    //TODO: If it's not a string, then it's a json formatted value -
                                    // we cannot really index this in a smart way since it could be 'anything'
                                    if (areaVal.Type == JTokenType.String)
                                    {
                                        var str = areaVal.Value<string>();
                                        str = XmlHelper.CouldItBeXml(str) ? str.StripHtml() : str;
                                        sb.Append(str);
                                        sb.Append(" ");

                                        //add the row name as an individual field
                                        e.Document.Add(
                                            new Field(
                                                $"{value.Key}.{rowName}", str, Field.Store.YES, Field.Index.ANALYZED));
                                    }

                                }
                            }

                            if (sb.Length > 0)
                            {
                                //First save the raw value to a raw field
                                e.Document.Add(
                                    new Field(
                                        $"{UmbracoExamineIndexer.RawFieldPrefix}{value.Key}",
                                        firstVal, Field.Store.YES, Field.Index.NO, Field.TermVector.NO));

                                //now replace the original value with the combined/cleaned value
                                e.Document.RemoveField(value.Key);
                                e.Document.Add(
                                    new Field(
                                        value.Key,
                                        sb.ToString(), Field.Store.YES, Field.Index.ANALYZED));
                            }
                        }
                    }
                    catch (InvalidCastException)
                    {
                        //swallow...on purpose, there's a chance that this isn't the json format we are looking for
                        // and we don't want that to affect the website.
                    }
                    catch (JsonException)
                    {
                        //swallow...on purpose, there's a chance that this isn't json and we don't want that to affect
                        // the website.
                    }
                    catch (ArgumentException)
                    {
                        //swallow on purpose to prevent this error:
                        // Can not add Newtonsoft.Json.Linq.JValue to Newtonsoft.Json.Linq.JObject.
                    }
                }

            }
        }

        /// <summary>
        /// Overridden to ensure that the value is validated
        /// </summary>
        /// <returns></returns>
        protected override IDataValueEditor CreateValueEditor() => new GridPropertyValueEditor(Attribute);

        protected override IConfigurationEditor CreateConfigurationEditor() => new GridConfigurationEditor();

        internal class GridPropertyValueEditor : DataValueEditor
        {
            public GridPropertyValueEditor(DataEditorAttribute attribute)
                : base(attribute)
            { }
        }
    }
}
