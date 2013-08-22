using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.installer
{
    [Obsolete("This is no longer used and will be removed from the codebase in future versions. It has been superceded by Umbraco.Web.Install.InstallerStep however that is marked internal and is not to be used in external code.")]
    public abstract class InstallerStep : IInstallerStep
    {
        public abstract string Alias { get; }
        public abstract string Name { get; }
        public abstract string UserControl { get;}
        public virtual int Index { get; set; }

        public virtual bool MoveToNextStepAutomaticly { get; set; }

        public virtual bool HideFromNavigation {
          get {
            return false;
          }
        }
        public abstract bool Completed();

      
    }
}
