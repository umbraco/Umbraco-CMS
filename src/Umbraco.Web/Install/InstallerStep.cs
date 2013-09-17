

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

        /// <summary>
        /// Determine if the installer should skip this step, if it returns true then it is assumed this step is already complete.
        /// </summary>
        /// <returns></returns>
        public abstract bool Completed();


    }
}
