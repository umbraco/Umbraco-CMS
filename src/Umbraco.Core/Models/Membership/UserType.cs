using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents the Type for a back-office User
    /// </summary>    
    [Serializable]
    [DataContract(IsReference = true)]
    internal class UserType : UserTypeGroupBase, IUserType
    {
    }
}