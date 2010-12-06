using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace umbraco.cms.businesslogic.installer
{
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
          bool found = false;
          
          foreach(InstallerStep i in this.Values){
            if (found && !i.Completed()) {
              return i;
            }

            if (i.Alias == key)
              found = true;
          }
  
          return null;
        }
    

        public InstallerStep FirstAvailableStep()
        {
            return this.First(item => item.Value.Completed() == false ).Value;
        }
    }
}

