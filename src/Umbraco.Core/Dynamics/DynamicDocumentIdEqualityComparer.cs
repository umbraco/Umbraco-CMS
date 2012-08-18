using System;
using System.Collections.Generic;

namespace Umbraco.Core.Dynamics
{
    internal class DynamicDocumentIdEqualityComparer : EqualityComparer<DynamicDocument>
    {

        public override bool Equals(DynamicDocument x, DynamicDocument y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the nodes ids are equal.
            return x.Id == y.Id;

        }

        public override int GetHashCode(DynamicDocument obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashId = obj.Id.GetHashCode();

            return hashId;
        }

    }
}
