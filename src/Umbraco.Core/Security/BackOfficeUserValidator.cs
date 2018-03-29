using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// Custom validator to not validate a user's username or email if they haven't changed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BackOfficeUserValidator<T> : UserValidator<T, int>
        where T : BackOfficeIdentityUser
    {
        public BackOfficeUserValidator(UserManager<T, int> manager) : base(manager)
        {
        }

        public override async Task<IdentityResult> ValidateAsync(T item)
        {
            //Don't validate if the user's email or username hasn't changed otherwise it's just wasting SQL queries.
            if (item.IsPropertyDirty("Email") || item.IsPropertyDirty("UserName"))
            {
                return await base.ValidateAsync(item);
            }
            return IdentityResult.Success;
        }
    }
}