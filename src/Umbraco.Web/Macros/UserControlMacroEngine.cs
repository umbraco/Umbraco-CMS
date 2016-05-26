using System;
using System.IO;
using System.Web;
using System.Web.UI;
using umbraco;
using umbraco.cms.businesslogic.macro;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Macros
{
    class UserControlMacroEngine
    {
        public MacroContent Execute(MacroModel model)
        {
            var filename = model.TypeName;

            // ensure the file exists
            var path = IOHelper.FindFile(filename);
            if (File.Exists(IOHelper.MapPath(path)) == false)
                throw new UmbracoException($"Failed to load control, file {filename} does not exist.");

            // load the control
            var control = (UserControl)new UserControl().LoadControl(path);
            control.ID = string.IsNullOrEmpty(model.MacroControlIdentifier)
                ? GetControlUniqueId(filename)
                : model.MacroControlIdentifier;

            // initialize the control
            LogHelper.Info<UserControlMacroEngine>($"Loaded control \"{filename}\" with ID \"{control.ID}\".");
            //SetControlCurrentNode(control); // fixme what are the consequences?
            UpdateControlProperties(control, model);

            return new MacroContent { Control = control };
        }

        // no more INode in v8
        /*
        // sets the control CurrentNode|currentNode property
        private static void SetControlCurrentNode(Control control)
        {
            var node = GetCurrentNode(); // get the current INode
            SetControlCurrentNode(control, "CurrentNode", node);
            SetControlCurrentNode(control, "currentNode", node);
        }

        // sets the control 'propertyName' property, of type INode
        private static void SetControlCurrentNode(Control control, string propertyName, INode node)
        {
            var currentNodeProperty = control.GetType().GetProperty(propertyName);
            if (currentNodeProperty != null && currentNodeProperty.CanWrite &&
                currentNodeProperty.PropertyType.IsAssignableFrom(typeof(INode)))
            {
                currentNodeProperty.SetValue(control, node, null);
            }
        }
        */

        private static string GetControlUniqueId(string filename)
        {
            const string key = "MacroControlUniqueId";

            var x = 0;

            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Items.Contains(key))
                    x = (int)HttpContext.Current.Items[key];
                x += 1;
                HttpContext.Current.Items[key] = x;
            }

            return $"{Path.GetFileNameWithoutExtension(filename)}_{x}";
        }

        // set the control properties according to the model properties ie parameters
        internal static void UpdateControlProperties(Control control, MacroModel model)
        {
            var type = control.GetType();

            foreach (var modelProperty in model.Properties)
            {
                var controlProperty = type.GetProperty(modelProperty.Key);
                if (controlProperty == null)
                {
                    LogHelper.Warn<UserControlMacroEngine>($"Control property \"{modelProperty.Key}\" doesn't exist or isn't accessible, skip.");
                    continue;
                }

                var tryConvert = modelProperty.Value.TryConvertTo(controlProperty.PropertyType);
                if (tryConvert.Success)
                {
                    try
                    {
                        controlProperty.SetValue(control, tryConvert.Result, null);
                        LogHelper.Debug<UserControlMacroEngine>($"Set property \"{modelProperty.Key}\" value \"{modelProperty.Value}\".");
                    }
                    catch (Exception e)
                    {
                        LogHelper.WarnWithException<UserControlMacroEngine>($"Failed to set property \"{modelProperty.Key}\" value \"{modelProperty.Value}\".", e);
                    }
                }
                else
                {
                    LogHelper.Warn<UserControlMacroEngine>($"Failed to set property \"{modelProperty.Key}\" value \"{modelProperty.Value}\".");
                }
            }
        }
    }
}
