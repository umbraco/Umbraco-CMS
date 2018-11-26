using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Xml;
using Umbraco.Examine;

namespace Umbraco.Web.PropertyEditors
{
    using System.Collections.Generic;
    using Umbraco.Core.Models;

    /// <summary>
    /// Parses the grid value into indexable values
    /// </summary>
    public class GridValueIndexer : IValueIndexer
    {
        public IEnumerable<KeyValuePair<string, object[]>> GetIndexValues(Property property, string culture)
        {
            var result = new Dictionary<string, object[]>();

            var val = property.GetValue(culture);

            //if there is a value, it's a string and it's detected as json
            if (val is string rawVal && rawVal.DetectIsJson())
            {
                try
                {
                    //TODO: We should deserialize this to Umbraco.Core.Models.GridValue instead of doing the below
                    var json = JsonConvert.DeserializeObject<JObject>(rawVal);

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
                                    result.Add($"{property.Alias}.{rowName}", new[] { str });
                                }
                            }
                        }

                        if (sb.Length > 0)
                        {
                            //First save the raw value to a raw field
                            result.Add($"{UmbracoExamineIndexer.RawFieldPrefix}{property.Alias}", new[] { rawVal });

                            //index the property with the combined/cleaned value
                            result.Add(property.Alias, new[] { sb.ToString() });
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

            return result;
        }
    }
}
