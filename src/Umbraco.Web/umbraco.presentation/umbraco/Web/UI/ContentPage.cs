using System;

namespace umbraco.presentation.Web.UI
{
    /// <summary>
    /// Summary description for ContentPage.
    /// </summary>
    public class ContentPage : System.Web.UI.Page
    {
        private int _id = 0;

        public int UmbracoNodeId
        {
            set { _id = value; }
            get {return _id;}
        }
        public ContentPage()
        {
        }
    }
}
