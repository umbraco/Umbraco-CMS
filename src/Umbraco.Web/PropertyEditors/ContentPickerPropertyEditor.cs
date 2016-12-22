using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.ContentPickerAlias, "Content Picker", PropertyEditorValueTypes.Integer, "contentpicker", IsParameterEditor = true, Group = "Pickers")]
    public class ContentPickerPropertyEditor : PropertyEditor
    {

        public ContentPickerPropertyEditor()
        {
            _internalPreValues = new Dictionary<string, object>
            {
                {"startNodeId", "-1"},
                {"showOpenButton", "0"},
                {"showEditButton", "0"},
                {"showPathOnHover", "0"}
            };
        }

        private IDictionary<string, object> _internalPreValues;
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _internalPreValues; }
            set { _internalPreValues = value; }
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new ContentPickerPreValueEditor();
        }

        internal class ContentPickerPreValueEditor : PreValueEditor
        {
            private const string _startNodeAlias = "startNodeId";

            [PreValueField("showOpenButton", "Show open button", "boolean")]
            public string ShowOpenButton { get; set; }

            [PreValueField("showEditButton", "Show edit button (this feature is in preview!)", "boolean")]
            public string ShowEditButton { get; set; }

            [PreValueField(_startNodeAlias, "Start node", "treepicker")]
            public int StartNodeId { get; set; }

            [PreValueField("showPathOnHover", "Show path when hovering items", "boolean")]
            public bool ShowPathOnHover { get; set; }

            public override IDictionary<string, PreValue> ConvertEditorToDb(IDictionary<string, object> editorValue, PreValueCollection currentValue)
            {
                var processedEditorValues = new Dictionary<string, PreValue>();
                foreach (var preval in editorValue)
                {
                    var processedPreVal = new KeyValuePair<string, PreValue>(preval.Key, preval.Value == null ? null : new PreValue(preval.Value.ToString()));
                    if (preval.Key == _startNodeAlias && preval.Value != null)
                    {
                        int startNodeUniqueId;
                        if (int.TryParse(preval.Value.ToString(), out startNodeUniqueId))
                        {
                            // look up the node
                            var cs = ApplicationContext.Current.Services.ContentService;
                            var contentNode = cs.GetById(startNodeUniqueId);
                            if (contentNode != null)
                            {
                                processedPreVal = new KeyValuePair<string, PreValue>(preval.Key, new PreValue(contentNode.Key.ToString()));
                            }
                        }
                    }
                    processedEditorValues.Add(processedPreVal.Key, processedPreVal.Value);
                }
                return processedEditorValues;

                }

            public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
            {
                var defaultPreValCopy = new Dictionary<string, object>();
                if (defaultPreVals != null)
                {
                    defaultPreValCopy = new Dictionary<string, object>(defaultPreVals);
                }

                //we just need to merge the dictionaries now, the persisted will replace default.
                foreach (var item in persistedPreVals.PreValuesAsDictionary)
                {
                    //The persisted dictionary contains values of type PreValue which contain the ID and the Value, we don't care
                    // about the Id, just the value so ignore the id.

                    // the start node could be stored as a Guid, in that case we'll need to find its integer Id
                    if (item.Key == _startNodeAlias)
                    {
                        Guid startNodeUniqueId;
                        if (Guid.TryParse(item.Value.Value, out startNodeUniqueId))
                        {
                            // look up the node
                            var cs = ApplicationContext.Current.Services.ContentService;
                            var contentNode = cs.GetById(startNodeUniqueId);
                            if (contentNode != null)
                            {
                                defaultPreValCopy[item.Key] = contentNode.Id.ToString();
                            }
                            else
                            {
                                // the content couldn't be found - revert to the start node for the user
                                defaultPreValCopy[item.Key] = UmbracoContext.Current.Security.CurrentUser.StartContentId.ToString();
                            }
                        }
                        else
                        {
                            defaultPreValCopy[item.Key] = item.Value.Value;
                        }

                    }
                    else
                    {
                        defaultPreValCopy[item.Key] = item.Value.Value;
                    }

                }
                //now we're going to try to see if any of the values are JSON, if they are we'll convert them to real JSON objects
                // so they can be consumed as real json in angular!
                base.ConvertItemsToJsonIfDetected(defaultPreValCopy);

                return defaultPreValCopy;
            }
        }
    }
}