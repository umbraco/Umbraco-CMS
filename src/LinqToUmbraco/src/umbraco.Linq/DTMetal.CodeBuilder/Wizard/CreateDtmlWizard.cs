using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TemplateWizard;
using EnvDTE;
using System.Windows.Forms;

namespace umbraco.Linq.DTMetal.CodeBuilder
{
    public class CreateDtmlWizard : IWizard
    {
        private string _tool;
        private bool _ok;

        #region IWizard Members

        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
            if (projectItem == null)
            {
                throw new ArgumentNullException("projectitem");
            }

            projectItem.Properties.Item("CustomTool").Value = _tool;
        }

        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            if (runKind == WizardRunKind.AsNewItem)
            {
                var dte = automationObject as _DTE;
                var owner = new IDEWindow(new IntPtr(dte.MainWindow.HWnd));

                _tool = replacementsDictionary["$CustomTool$"];
                string dataContextName = null;
                if (replacementsDictionary.ContainsKey("$rootname$"))
                {
                    dataContextName = replacementsDictionary["$rootname$"];
                    if (dataContextName.EndsWith(".dtml", StringComparison.OrdinalIgnoreCase))
                    {
                        dataContextName = dataContextName.Substring(0, dataContextName.Length - ".dtml".Length);
                    }
                }
            }

            throw new NotImplementedException();
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return _ok;
        }

        #endregion
    }


    /// <summary>
    /// Helper class for the Visual Studio IDE window.
    /// </summary>
    internal class IDEWindow : IWin32Window
    {
        /// <summary>
        /// Handle to the IDE window.
        /// </summary>
        private IntPtr _handle;

        /// <summary>
        /// Creates a new Visual Studio IDE window wrapper.
        /// </summary>
        /// <param name="handle">Handle to the IDE window.</param>
        public IDEWindow(IntPtr handle)
        {
            _handle = handle;
        }

        /// <summary>
        /// Gets the handle to the IDE window.
        /// </summary>
        public IntPtr Handle
        {
            get { return _handle; }
        }
    }
}
