using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using CookComputing.MetaWeblog;
using CookComputing.XmlRpc;

namespace umbraco.presentation.channels
{

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct wpPage
    {
        public DateTime dateCreated;
        public int userid;
        public int page_id;
        public string page_status;
        public string description;
        public string title;
        public string link;
        public string permalink;
        public string[] categories;
        public string excerpt;
        public string text_more;
        public int mt_allow_comments;
        public int mt_allow_pings;

        public string wp_slug;
        public string wp_password;
        public string wp_author;
        public int wp_page_parent_id;
        public string wp_page_parent_title;
        public int wp_page_order;
        public int wp_author_id;
        public string wp_author_display_name;

    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct wpPageSummary
    {
        public int page_id;
        public string page_title;
        public int page_parent_id;
        public DateTime dateCreated;

    }
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct wpCategory
    {
        public string name;
        public string parent_id;
    }


    public interface IRemixWeblogApi
    {
        [XmlRpcMethod("wp.getPageList",
            Description = "Retrieves a list of pages as summary from the current channel")]
        wpPageSummary[] getPageList(
            string blogid,
            string username,
            string password);

        [XmlRpcMethod("wp.getPages",
            Description = "Retrieves a list of pages from the current channel")]
        wpPage[] getPages(
            string blogid,
            string username,
            string password,
            int numberOfItems);

        [XmlRpcMethod("wp.newCategory",
            Description = "Adds a new category")]
        string newCategory(
            string blogid,
            string username,
            string password,
            wpCategory category);

    }

}
