using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace umbraco.cms.businesslogic.installer
{

    [Obsolete("This is no longer used and will be removed from the codebase in future versions. It has been superceded by Umbraco.Web.Install.InstallerStepCollection however that is marked internal and is not to be used in external code.")]
    public class InstallerStepCollection : Dictionary<string,InstallerStep>
    {
        public void Add(InstallerStep step){
            step.Index = this.Count;
            this.Add(step.Alias, step);
        }
  
        public InstallerStep Get(string key)
        {
            return this.First(item => item.Key == key).Value;
        }

        public bool StepExists(string key)
        {
            return this.ContainsKey(key);
        }

        public InstallerStep GotoNextStep(string key)
        {
          InstallerStep s = this[key];
          foreach(InstallerStep i in this.Values){

             // System.Web.HttpContext.Current.Response.Write(i.Index.ToString() + i.Alias);

            if (i.Index > s.Index && !i.Completed()) {
               // System.Web.HttpContext.Current.Response.Write( "FOUND" +  i.Index.ToString() + i.Alias);
                return i;
            }
          }
  
          return null;
        }
    

        public InstallerStep FirstAvailableStep()
        {
            return this.First(item => item.Value.Completed() == false ).Value;
        }
    }
}

