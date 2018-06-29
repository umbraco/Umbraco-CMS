using System;
using System.IO;
using System.Web;
using System.Web.UI;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Macros
{
    class UserControlMacroEngine
    {
        public MacroContent Execute(MacroModel model)
        {
            var filename = model.MacroSource;

            // ensure the file exists
            var path = IOHelper.FindFile(filename);
            if (File.Exists(IOHelper.MapPath(path)) == false)
                throw new Exception($"Failed to load control, file {filename} does not exist.");

            // load the control
            var control = (UserControl)new UserControl().LoadControl(path);
            control.ID = string.IsNullOrEmpty(model.MacroControlIdentifier)
                ? GetControlUniqueId(filename)
                : model.MacroControlIdentifier;

            // initialize the control
            // note: we are not setting the 'CurrentNode' property on the control anymore,
            // as that was an INode which is gone in v8. Use UmbracoContext to access the
            // current content.
            Current.Logger.Info<UserControlMacroEngine>(() => $"Loaded control \"{filename}\" with ID \"{control.ID}\".");
            UpdateControlProperties(control, model);

            return new MacroContent { Control = control };
        }

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
                    Current.Logger.Warn<UserControlMacroEngine>($"Control property \"{modelProperty.Key}\" doesn't exist or isn't accessible, skip.");
                    continue;
                }

                var tryConvert = modelProperty.Value.TryConvertTo(controlProperty.PropertyType);
                if (tryConvert.Success)
                {
                    try
                    {
                        controlProperty.SetValue(control, tryConvert.Result, null);
                        Current.Logger.Debug<UserControlMacroEngine>(() => $"Set property \"{modelProperty.Key}\" value \"{modelProperty.Value}\".");
                    }
                    catch (Exception e)
                    {
                        Current.Logger.Warn<UserControlMacroEngine>(e, $"Failed to set property \"{modelProperty.Key}\" value \"{modelProperty.Value}\".");
                    }
                }
                else
                {
                    Current.Logger.Warn<UserControlMacroEngine>($"Failed to set property \"{modelProperty.Key}\" value \"{modelProperty.Value}\".");
                }
            }
        }
    }
}
