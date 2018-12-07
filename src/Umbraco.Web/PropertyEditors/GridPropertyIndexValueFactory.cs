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
    using System.Linq;
    using Umbraco.Core.Models;

    /// <summary>
    /// Parses the grid value into indexable values
    /// </summary>
    public class GridPropertyIndexValueFactory : IPropertyIndexValueFactory
    {
        public IEnumerable<KeyValuePair<string, IEnumerable<object>>> GetIndexValues(Property property, string culture, string segment, bool published)
        {
            var result = new List<KeyValuePair<string, IEnumerable<object>>>();

            var val = property.GetValue(culture, segment, published);

            //if there is a value, it's a string and it's detected as json
            if (val is string rawVal && rawVal.DetectIsJson())
            {
                try
                {
                    var gridVal = JsonConvert.DeserializeObject<GridValue>(rawVal);

                    //get all values and put them into a single field (using JsonPath)
                    var sb = new StringBuilder();
                    foreach (var row in gridVal.Sections.SelectMany(x => x.Rows))
                    {
                        var rowName = row.Name;

                        foreach (var control in row.Areas.SelectMany(x => x.Controls))
                        {
                            var controlVal = control.Value;

                            //TODO: If it's not a string, then it's a json formatted value -
                            // we cannot really index this in a smart way since it could be 'anything'
                            if (controlVal.Type == JTokenType.String)
                            {
                                var str = controlVal.Value<string>();
                                str = XmlHelper.CouldItBeXml(str) ? str.StripHtml() : str;
                                sb.Append(str);
                                sb.Append(" ");

                                //add the row name as an individual field
                                result.Add(new KeyValuePair<string, IEnumerable<object>>($"{property.Alias}.{rowName}", new[] { str }));
                            }
                        }
                    }

                    if (sb.Length > 0)
                    {
                        //First save the raw value to a raw field
                        result.Add(new KeyValuePair<string, IEnumerable<object>>($"{UmbracoExamineIndexer.RawFieldPrefix}{property.Alias}", new[] { rawVal }));

                        //index the property with the combined/cleaned value
                        result.Add(new KeyValuePair<string, IEnumerable<object>>(property.Alias, new[] { sb.ToString() }));
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
