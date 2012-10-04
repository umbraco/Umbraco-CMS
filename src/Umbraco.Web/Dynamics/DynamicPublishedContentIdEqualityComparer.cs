using System;
using System.Collections.Generic;
using Umbraco.Web.Models;

namespace Umbraco.Web.Dynamics
{
    internal class DynamicPublishedContentIdEqualityComparer : EqualityComparer<DynamicPublishedContent>
    {

        public override bool Equals(DynamicPublishedContent x, DynamicPublishedContent y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the nodes ids are equal.
            return x.Id == y.Id;

        }

        public override int GetHashCode(DynamicPublishedContent obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashId = obj.Id.GetHashCode();

            return hashId;
        }

    }
}
