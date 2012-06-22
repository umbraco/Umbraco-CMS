#region namespace
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;
using System.Configuration;
using umbraco.BusinessLogic;
using System.Security.Cryptography;
using System.Web.Util;
using System.Collections.Specialized;
using System.Configuration.Provider;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using System.Collections;
using System.Web.Profile;
#endregion

namespace umbraco.providers.members {
    public class UmbracoProfileProvider : ProfileProvider {

        private string m_ApplicationName = "";

        public override string ApplicationName {
            get {
                return m_ApplicationName;
            }
            set {
                m_ApplicationName = value;
            }
        }
        public override string Description {
            get {
                return "Profile Provider for umbraco member profile data";
            }
        }
        public override string Name {
            get {
                return base.Name;
            }
        }
        public override void Initialize(string name, NameValueCollection config) {

            if (config == null)
                throw new ArgumentNullException("Null configuration parameters");

            if (String.IsNullOrEmpty(name))
                name = "UmbracoProfileProvider";

            base.Initialize(name, config);

            m_ApplicationName = config["applicationName"];
            if (String.IsNullOrEmpty(m_ApplicationName))
                m_ApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            config.Remove("applicationName");

            // if the config element contains unused parameters we should throw an exception
            if (config.Count > 0) { 
                string attrib = config.GetKey(0);
                if (!String.IsNullOrEmpty(attrib))
                    throw new ProviderException(String.Format("Unrecognized attribute: {0}", attrib));
            }


        }
        
        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate) {
            throw new NotSupportedException();
        }

        public override int DeleteProfiles(string[] usernames) {
            throw new NotSupportedException();
        }

        public override int DeleteProfiles(ProfileInfoCollection profiles) {
            throw new NotSupportedException();
        }

        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords) {
            throw new NotSupportedException();
        }

        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) {
            throw new NotSupportedException();
        }

        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords) {
            throw new NotSupportedException();
        }
        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords) {
            throw new NotSupportedException();
        }
        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate) {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Returns the collection of settings property values for the current umbraco member.
        /// </summary>
        /// <param name="context">A <see cref="T:System.Configuration.SettingsContext"/> describing the current application use.</param>
        /// <param name="collection">A <see cref="T:System.Configuration.SettingsPropertyCollection"/> containing the settings property group whose values are to be retrieved.</param>
        /// <returns>
        /// A <see cref="T:System.Configuration.SettingsPropertyValueCollection"/> containing the values for the specified settings property group.
        /// </returns>
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection) {
            SettingsPropertyValueCollection settings = new SettingsPropertyValueCollection();

            if (collection.Count == 0)
                return settings;

            foreach(SettingsProperty property in collection){
                SettingsPropertyValue pv = new SettingsPropertyValue(property);
                settings.Add(pv);
            }

            // get the current user
            string username = (string)context["UserName"];
            Member m = Member.GetMemberFromLoginName(username);
            if (m == null)
                throw new ProviderException(String.Format("No member with username '{0}' exists", username));

            foreach (SettingsPropertyValue spv in settings) {
                if (m.getProperty(spv.Name) != null) {
                    spv.Deserialized = true;
                    spv.PropertyValue = m.getProperty(spv.Name).Value;
                }
            }

            return settings;

        }

        /// <summary>
        /// Sets the values of the specified group of property settings for the current umbraco member.
        /// </summary>
        /// <param name="context">A <see cref="T:System.Configuration.SettingsContext"/> describing the current application usage.</param>
        /// <param name="collection">A <see cref="T:System.Configuration.SettingsPropertyValueCollection"/> representing the group of property settings to set.</param>
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection) {

            string username = (string)context["UserName"];
            bool authenticated = (bool)context["IsAuthenticated"];

            if (String.IsNullOrEmpty(username) || collection.Count == 0)
                return;

            Member m = Member.GetMemberFromLoginName(username);
            if (m == null)
                throw new ProviderException(String.Format("No member with username '{0}' exists", username));


            foreach (SettingsPropertyValue spv in collection) {
                if (!authenticated && !(bool)spv.Property.Attributes["AllowAnonymous"])
                    continue;

                if (m.getProperty(spv.Name) != null)
                    m.getProperty(spv.Name).Value = spv.PropertyValue;
            }

        }
    }
}
