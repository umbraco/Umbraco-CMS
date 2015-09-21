using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using Umbraco.Core;
using umbraco.BusinessLogic;
using umbraco.businesslogic.Exceptions;
using umbraco.interfaces;

namespace Umbraco.Web.UI
{
    /// <summary>
    /// An abstract class that is used to implement all secure ITasks
    /// </summary>
    /// <remarks>
    /// In the near future we will overhaul how create dialogs work and how deletions work as well. In the meantime 
    /// if you ever need to create an ITask you should just inherit from this class and do not manually implement 
    /// ITask or ITaskReturnUrl. If you do, you MUST also implement IAppTask which associates an ITask to an app
    /// so we can validate the current user's security with the implementation. If you do not do this then your 
    /// implementation will not be secure. It means that if someone is logged in and doesn't have access to a 
    /// specific app, they'd still be able to execute code to create/delete for any ITask regardless of what app 
    /// they have access to.
    /// </remarks>
    [Obsolete("ITask is used for legacy webforms back office editors, change to using the v7 angular approach")]
    public abstract class LegacyDialogTask : ITaskReturnUrl, IAssignedApp
    {
        public virtual int ParentID { get; set; }
        public int TypeID { get; set; }
        public string Alias { get; set; }
        
        /// <summary>
        /// Base class first performs authentication for the current app before proceeding
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            if (ValidateUserForApplication())
            {
                return PerformSave();                
            }
            throw new AuthenticationException("The current user does not have access to the required application that this task belongs to");
        }

        public abstract bool PerformSave();

        /// <summary>
        /// Base class first performs authentication for the current app before proceeding
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            if (ValidateUserForApplication())
            {
                return PerformDelete();
            }
            throw new AuthenticationException("The current user does not have access to the required application that this task belongs to");
        }

        public abstract bool PerformDelete();

        /// <summary>
        /// Gets/sets the user object for this Task
        /// </summary>
        /// <remarks>
        /// accessible by inheritors but can only be set internally
        /// </remarks>
        protected internal User User { get; internal set; }

        /// <summary>
        /// Implemented explicitly as we don't want to expose this
        /// </summary>
        int ITask.UserId
        {
            set { User = User.GetUser(value); }
        }

        /// <summary>
        /// Checks if the currently assigned user has access to the assigned app
        /// </summary>
        /// <returns></returns>
        protected internal bool ValidateUserForApplication()
        {
            if (User == null)
                throw new InvalidOperationException("Cannot authenticate, no User object assigned");

            return User.Applications.Any(app => app.alias.InvariantEquals(AssignedApp));
        }

        public abstract string ReturnUrl { get; }
        public abstract string AssignedApp { get; }

        public IDictionary<string, object> AdditionalValues { get; set; }
    }
}
