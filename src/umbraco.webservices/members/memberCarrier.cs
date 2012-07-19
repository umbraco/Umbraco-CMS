using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace umbraco.webservices.members
{
    [Serializable]
    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    public class memberCarrier
    {

        public memberCarrier()
        {
            memberProperties = new List<memberProperty>();
            groups = new List<memberGroup>();
        }

        #region Fields
        private int id;

        private string password;
        private string email;
        private string displayedName;
        private string loginName;

        private int membertypeId;
        private string membertypeName;

        private List<memberGroup> groups;
        private List<memberProperty> memberProperties;
        #endregion

        #region Properties

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public string DisplayedName
        {
            get { return displayedName; }
            set { displayedName = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        public string LoginName
        {
            get { return loginName; }
            set { loginName = value; }
        }

        public int MembertypeId
        {
            get { return membertypeId; }
            set { membertypeId = value; }
        }


        public string MembertypeName
        {
            get { return membertypeName; }
            set { membertypeName = value; }
        }

        public List<memberGroup> Groups
        {
            get { return groups; }
            set { groups = value; }
        }

        public List<memberProperty> MemberProperties
        {
            get { return memberProperties; }
            set { memberProperties = value; }
        }

        #endregion
    }
}