using System;
using Umbraco.Core.Models.Membership;

namespace umbraco.cms.businesslogic.web
{
    /// <summary>
    /// A lightweight datastructure used to represent a version of a document
    /// </summary>
    public class DocumentVersionList
    {
        private Guid _version;
        private DateTime _date;
        private string _text;
        private IUser _user;

        /// <summary>
        /// The unique id of the version
        /// </summary>
        public Guid Version
        {
            get { return _version; }
        }

        /// <summary>
        /// The date of the creation of the version 
        /// </summary>
        public DateTime Date
        {
            get { return _date; }
        }

        /// <summary>
        /// The name of the document in the version
        /// </summary>
        public string Text
        {
            get { return _text; }
        }

        /// <summary>
        /// The user which created the version
        /// </summary>
        public IUser User
        {
            get { return _user; }
        }

        /// <summary>
        /// Initializes a new instance of the DocumentVersionList class.
        /// </summary>
        /// <param name="Version">Unique version id</param>
        /// <param name="Date">Version createdate</param>
        /// <param name="Text">Version name</param>
        /// <param name="User">Creator</param>
        public DocumentVersionList(Guid Version, DateTime Date, string Text, IUser User)
        {
            _version = Version;
            _date = Date;
            _text = Text;
            _user = User;
        }
    }
}
