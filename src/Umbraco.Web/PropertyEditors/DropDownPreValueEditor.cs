using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    internal class DropDownPreValueEditor : PreValueEditor
    {
        /// <summary>
        /// The editor is expecting a json array for a field with a key named "temp" so we need to format the persisted values
        /// to this format to be used in the editor.
        /// </summary>
        /// <param name="defaultPreVals"></param>
        /// <param name="persistedPreVals"></param>
        /// <returns></returns>
        public override IDictionary<string, object> FormatDataForEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
        {
            var dictionary = PreValueCollection.AsDictionary(persistedPreVals);
            var arrayOfVals = dictionary.Select(item => item.Value).ToList();
            var json = JsonConvert.SerializeObject(arrayOfVals);

            return new Dictionary<string, object> {{"temp", json}};
        }

        /// <summary>
        /// Need to format the delimited posted string to individual values
        /// </summary>
        /// <param name="editorValue"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is mostly because we want to maintain compatibility with v6 drop down property editors that store their prevalues in different db rows.
        /// </remarks>
        public override IDictionary<string, string> FormatDataForPersistence(IDictionary<string, string> editorValue, Core.Models.PreValueCollection currentValue)
        {
            var val = editorValue["temp"];
            var result = new Dictionary<string, string>();
            if (val.IsNullOrWhiteSpace()) return result;

            try
            {
                var deserialized = JsonConvert.DeserializeObject<string[]>(val);
                var index = 0;
                foreach (var item in deserialized)
                {
                    result.Add(index.ToInvariantString(), item);
                    index++;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<DropDownPreValueEditor>("Could not deserialize the posted value: " + val, ex);                
            }

            return result;
        }
    }
}