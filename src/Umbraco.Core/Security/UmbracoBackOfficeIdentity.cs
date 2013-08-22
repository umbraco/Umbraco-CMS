using System;
using System.Web;
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
    internal class UmbracoBackOfficeIdentity : FormsIdentity
    {
        public UmbracoBackOfficeIdentity(FormsAuthenticationTicket ticket) 
            : base(ticket)
        {
            UserData = ticket.UserData;
            EnsureDeserialized();
        }
        
        protected readonly string UserData;
        internal UserData DeserializedData;

        public string UserContextId
        {
            get { return DeserializedData.UserContextId; }
        }

        public int StartContentNode
        {
            get { return DeserializedData.StartContentNode; }
        }

        public int StartMediaNode
        {
            get { return DeserializedData.StartMediaNode; }
        }

        public string[] AllowedApplications
        {
            get { return DeserializedData.AllowedApplications; }
        }
        
        public object Id
        {
            get { return DeserializedData.Id; }
        }

        public string RealName
        {
            get { return DeserializedData.RealName; }
        }

        public string Culture
        {
            get { return DeserializedData.Culture; }
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
            get { return DeserializedData.Roles; }
        }

        /// <summary>
        /// This will ensure we only deserialize once
        /// </summary>
        /// <remarks>
        /// For performance reasons, we'll also check if there's an http context available,
        /// if so, we'll chuck our instance in there so that we only deserialize once per request.
        /// </remarks>
        protected void EnsureDeserialized()
        {
            if (DeserializedData != null)
                return;

            if (HttpContext.Current != null)
            {
                //check if we've already done this in this request
                var data = HttpContext.Current.Items[typeof(UmbracoBackOfficeIdentity)] as UserData;
                if (data != null)
                {
                    DeserializedData = data;
                    return;
                }
            }

            if (string.IsNullOrEmpty(UserData))
            {
                throw new NullReferenceException("The " + typeof(UserData) + " found in the ticket cannot be empty");
            }
            DeserializedData = JsonConvert.DeserializeObject<UserData>(UserData);
            
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items[typeof (UmbracoBackOfficeIdentity)] = DeserializedData;
            }
        }
    }
}