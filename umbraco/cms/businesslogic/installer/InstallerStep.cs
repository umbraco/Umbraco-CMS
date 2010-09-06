using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.installer
{
    public abstract class InstallerStep : umbraco.cms.businesslogic.installer.IInstallerStep
    {
        public abstract string Alias { get; }
        public abstract string Name { get; }
        public abstract string UserControl { get;}
        public virtual int Index { get; set; }

        public virtual bool MoveToNextStepAutomaticly { get; set; }
        public virtual bool HideNextButtonUntillCompleted { get; set; }

        public abstract bool Completed();

        public virtual string NextStep(){
            return "meh";
        }
    }
}
