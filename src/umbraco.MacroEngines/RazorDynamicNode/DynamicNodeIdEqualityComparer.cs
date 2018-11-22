using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.MacroEngines
{
    public class DynamicNodeIdEqualityComparer : EqualityComparer<DynamicNode>
    {

        public override bool Equals(DynamicNode x, DynamicNode y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the nodes ids are equal.
            return x.Id == y.Id;

        }

        public override int GetHashCode(DynamicNode obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashId = obj.Id.GetHashCode();

            return hashId;
        }

    }
}
