using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Members
{
    /// <summary>
    /// An Umbraco member user type
    /// </summary>
    public class UmbracoMembersIdentityUser : IdentityUser<int, IIdentityUserLogin, IdentityUserRole<string>, IdentityUserClaim<int>>, IRememberBeingDirty
    {
        private readonly BeingDirty _beingDirty = new BeingDirty();

        #region BeingDirty
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                _beingDirty.PropertyChanged += value;
            }
            remove
            {
                _beingDirty.PropertyChanged -= value;
            }
        }

        public void DisableChangeTracking()
        {
            throw new NotImplementedException();
        }

        public void EnableChangeTracking()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetDirtyProperties()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetWereDirtyProperties()
        {
            throw new NotImplementedException();
        }

        public bool IsDirty()
        {
            throw new NotImplementedException();
        }

        public bool IsPropertyDirty(string propName)
        {
            throw new NotImplementedException();
        }

        public void ResetDirtyProperties(bool rememberDirty)
        {
            throw new NotImplementedException();
        }

        public void ResetDirtyProperties()
        {
            throw new NotImplementedException();
        }

        public void ResetWereDirtyProperties()
        {
            throw new NotImplementedException();
        }

        public bool WasDirty()
        {
            throw new NotImplementedException();
        }

        public bool WasPropertyDirty(string propertyName)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
