using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using umbraco.interfaces;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace umbraco.businesslogic.Actions
{
    [ActionMetadata(Constants.Conventions.PermissionCategories.ContentCategory)]
    public class ActionCreateBlueprintFromContent : IAction
    {
        private static readonly ActionCreateBlueprintFromContent instance = new ActionCreateBlueprintFromContent();

        public static ActionCreateBlueprintFromContent Instance
        {
            get { return instance; }
        }

        public char Letter { get; private set; }
        public bool ShowInNotifier { get; private set; }
        public bool CanBePermissionAssigned { get; private set; }
        public string Icon { get; private set; }
        public string Alias { get; private set; }
        public string JsFunctionName { get; private set; }
        public string JsSource { get; private set; }

        public ActionCreateBlueprintFromContent()
        {
            Letter = 'ï';
            CanBePermissionAssigned = true;
            Icon = "blueprint";
            Alias = "createblueprint";
        }
        
    }
}
