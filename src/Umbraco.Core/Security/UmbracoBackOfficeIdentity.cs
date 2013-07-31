using System.Web.Security;
using Newtonsoft.Json;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// A custom user identity for the Umbraco backoffice
    /// </summary>
    /// <remarks>
    /// All values are lazy loaded for performance reasons as the constructor is called for every single request
    /// </remarks>
    public class UmbracoBackOfficeIdentity : FormsIdentity
    {
        public UmbracoBackOfficeIdentity(FormsAuthenticationTicket ticket) 
            : base(ticket)
        {
            UserData = ticket.UserData;
        }
        
        protected readonly string UserData;
        internal UserData DeserializedData;

        public int StartContentNode
        {
            get
            {
                EnsureDeserialized();
                return DeserializedData.StartContentNode;
            }
        }

        public int StartMediaNode
        {
            get
            {
                EnsureDeserialized();
                return DeserializedData.StartMediaNode;
            }
        }

        public string[] AllowedApplications
        {
            get
            {
                EnsureDeserialized();
                return DeserializedData.AllowedApplications;
            }
        }
        
        public int Id
        {
            get
            {
                EnsureDeserialized();
                return DeserializedData.Id;
            }
        }

        public string RealName
        {
            get
            {
                EnsureDeserialized();
                return DeserializedData.RealName;
            }
        }

        //public int SessionTimeout
        //{
        //    get
        //    {
        //        EnsureDeserialized();
        //        return DeserializedData.SessionTimeout;
        //    }
        //}

        public string[] Roles
        {
            get
            {
                EnsureDeserialized();
                return DeserializedData.Roles;                
            }
        }

        protected void EnsureDeserialized()
        {
            if (DeserializedData != null)
                return;
            
            if (string.IsNullOrEmpty(UserData))
            {
                DeserializedData = new UserData();
                return;
            }
            DeserializedData = JsonConvert.DeserializeObject<UserData>(UserData);            
        }
    }
}