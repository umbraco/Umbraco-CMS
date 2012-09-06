using System;

namespace umbraco.webservices.members
{
    [Serializable]
    public class memberGroup
    {
        int groupID;
        string groupName;

        public memberGroup()
        {
        }

        public memberGroup(int groupID, string groupName)
        {
            GroupID = groupID;
            GroupName = groupName;
        }

        public int GroupID
        {
            get { return groupID; }
            set { groupID = value; }
        }

        public string GroupName
        {
            get { return groupName; }
            set { groupName = value; }
        }

    }
}