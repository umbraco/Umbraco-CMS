// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Xml;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Parses the grid value into indexable values
    /// </summary>
    public class GridPropertyIndexValueFactory : IPropertyIndexValueFactory
    {
        public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(IProperty property, string? culture, string? segment, bool published)
        {
            var result = new List<KeyValuePair<string, IEnumerable<object?>>>();

            var val = property.GetValue(culture, segment, published);

            //if there is a value, it's a string and it's detected as json
            if (val is string rawVal && rawVal.DetectIsJson())
            {
                try
                {
                    GridValue? gridVal = JsonConvert.DeserializeObject<GridValue>(rawVal);

                    //get all values and put them into a single field (using JsonPath)
                    var sb = new StringBuilder();
                    foreach (GridValue.GridRow row in gridVal!.Sections.SelectMany(x => x.Rows))
                    {
                        var rowName = row.Name;

                        foreach (GridValue.GridControl control in row.Areas.SelectMany(x => x.Controls))
                        {
                            JToken? controlVal = control.Value;

                            if (controlVal?.Type == JTokenType.String)
                            {
                                var str = controlVal.Value<string>();
                                str = XmlHelper.CouldItBeXml(str) ? str!.StripHtml() : str;
                                sb.Append(str);
                                sb.Append(" ");

                                //add the row name as an individual field
                                result.Add(new KeyValuePair<string, IEnumerable<object?>>($"{property.Alias}.{rowName}", new[] { str }));
                            }
                            else if (controlVal is JContainer jc)
                            {
                                foreach (JToken s in jc.Descendants().Where(t => t.Type == JTokenType.String))
                                {
                                    sb.Append(s.Value<string>());
                                    sb.Append(" ");
                                }
                            }
                        }
                    }

                    //First save the raw value to a raw field
                    result.Add(new KeyValuePair<string, IEnumerable<object?>>($"{UmbracoExamineFieldNames.RawFieldPrefix}{property.Alias}", new[] { rawVal }));

                    if (sb.Length > 0)
                    {
                        //index the property with the combined/cleaned value
                        result.Add(new KeyValuePair<string, IEnumerable<object?>>(property.Alias, new[] { sb.ToString() }));
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
