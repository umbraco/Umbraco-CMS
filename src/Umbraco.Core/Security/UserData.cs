using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// Data structure used to store information in the authentication cookie
    /// </summary>
    [DataContract(Name = "userData", Namespace = "")]
    [Serializable]
    public class UserData
    {
        public UserData()
        {
            AllowedApplications = new string[] {};
            Roles = new string[] {};
        }

        /// <summary>
        /// Use this constructor to create/assign new UserData to the ticket
        /// </summary>
        /// <param name="sessionId">
        /// The current sessionId for the user
        /// </param>
        public UserData(string sessionId)
        {
            SessionId = sessionId;
            AllowedApplications = new string[] { };
            Roles = new string[] { };
        }

        /// <summary>
        /// This is the 'sessionId' for validation
        /// </summary>        
        [DataMember(Name = "sessionId")]
        public string SessionId { get; set; }

        /// <summary>
        /// This is the 'security stamp' for validation
        /// </summary>        
        [DataMember(Name = "securityStamp")]
        public string SecurityStamp { get; set; }

        [DataMember(Name = "id")]
        public object Id { get; set; }
        
        [DataMember(Name = "roles")]
        public string[] Roles { get; set; }

        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "name")]
        public string RealName { get; set; }

        /// <summary>
        /// The start nodes on the UserData object for the auth ticket contains all of the user's start nodes including ones assigned to their user groups
        /// </summary>
        [DataMember(Name = "startContent")]
        public int[] StartContentNodes { get; set; }

        /// <summary>
        /// The start nodes on the UserData object for the auth ticket contains all of the user's start nodes including ones assigned to their user groups
        /// </summary>
        [DataMember(Name = "startMedia")]
        public int[] StartMediaNodes { get; set; }

        [DataMember(Name = "allowedApps")]
        public string[] AllowedApplications { get; set; }

        [DataMember(Name = "culture")]
        public string Culture { get; set; }
    }
}