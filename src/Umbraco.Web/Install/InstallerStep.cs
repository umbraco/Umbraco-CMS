

namespace Umbraco.Web.Install
{
    internal abstract class InstallerStep 
    {
        public abstract string Alias { get; }
        public abstract string Name { get; }
        public abstract string UserControl { get; }
        public virtual int Index { get; set; }

        public virtual bool MoveToNextStepAutomaticly { get; set; }

        public virtual bool HideFromNavigation
        {
            get
            {
                return false;
            }
        }
        public abstract bool Completed();


    }
}
