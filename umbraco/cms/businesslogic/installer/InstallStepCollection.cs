using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace umbraco.cms.businesslogic.installer
{
    public class InstallerStepCollection : List<KeyValuePair<string,InstallerStep>>
    {
        public void Add(InstallerStep step){
            step.Index = this.Count;
            KeyValuePair<string, InstallerStep> kv = new KeyValuePair<string, InstallerStep>(step.Alias, step);
            this.Add(kv);
        }

        public InstallerStep Get(int index)
        {
            return this[index].Value;
        }

        public InstallerStep Get(string key)
        {
            return this.First(item => item.Key == key).Value;
        }

        public bool StepExists(string key)
        {
            return this.Exists(item => item.Key == key);
        }

        public InstallerStep GotoNextStep(string key)
        {
            InstallerStep s = this.Get(key);
            for (int i = s.Index+1; i < this.Count; i++)
            {
                InstallerStep next = this[i].Value;
                if (!next.Completed())
                    return next;
            }

            return null;
        }
    

        public InstallerStep FirstAvailableStep()
        {
            return this.First(item => item.Value.Completed() == false ).Value;
        }
    }
}

