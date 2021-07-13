using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;

namespace Umbraco.Web.Actions
{
    public class ActionNotify : IAction
    {
        public char Letter => 'N';

        public bool ShowInNotifier => false;

        public bool CanBePermissionAssigned => true;

        public string Icon => "megaphone";

        public string Alias => "notify";

        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
    }
}
