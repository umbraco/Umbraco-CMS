using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.cms.businesslogic.index {

    /// <summary>
    /// Serializable access to items in the umbraco lucene index
    /// </summary>
    [Serializable]
    public class SearchItem {

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchItem"/> class.
        /// </summary>
        public SearchItem() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchItem"/> class.
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <param name="title">The title.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="description">The description.</param>
        /// <param name="author">The author.</param>
        /// <param name="changeDate">The change date.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="tags">The tags.</param>
        public SearchItem(int nodeId, string title, string icon, string description, string author, DateTime changeDate, Guid objectType, string[] tags) {
            m_nodeId = nodeId;
            m_Title = title;
            m_icon = icon;
            m_description = description;
            m_author = author;
            m_tags = tags;
        }

        private int m_nodeId;

        /// <summary>
        /// Gets or sets the node id.
        /// </summary>
        /// <value>The node id.</value>
        public int NodeId {
            get { return m_nodeId; }
            set { m_nodeId = value; }
        }

        private string m_Title;

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title {
            get { return m_Title; }
            set { m_Title = value; }
        }

        private Guid m_objectType;

        /// <summary>
        /// Gets or sets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        public Guid ObjectType {
            get { return m_objectType; }
            set { m_objectType = value; }
        }

        private DateTime m_changeDate;

        /// <summary>
        /// Gets or sets the change date.
        /// </summary>
        /// <value>The change date.</value>
        public DateTime ChangeDate {
            get { return m_changeDate; }
            set { m_changeDate = value; }
        }
	

        private string m_icon;

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        public string Icon {
            get { return m_icon; }
            set { m_icon = value; }
        }

        private string m_description;

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description {
            get { return m_description; }
            set { m_description = value; }
        }

        private string m_author;

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        /// <value>The author.</value>
        public string Author {
            get { return m_author; }
            set { m_author = value; }
        }

        private string[] m_tags;

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>The tags.</value>
        public string[] Tags {
            get { return m_tags; }
            set { m_tags = value; }
        }






    }

}
