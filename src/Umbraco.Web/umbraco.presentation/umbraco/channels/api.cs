using System;
using System.Collections;
using CookComputing.MetaWeblog;
using CookComputing.XmlRpc;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.channels.businesslogic;

namespace umbraco.presentation.channels
{
    /// <summary>
    /// the umbraco channels API is xml-rpc webservice based on the metaweblog and blogger APIs
    /// for editing umbraco data froom external clients
    /// </summary>
    [XmlRpcService(
        Name = "umbraco metablog api",
        Description = "For editing umbraco data from external clients",
        AutoDocumentation = true)]
    public class api : UmbracoMetaWeblogAPI, IRemixWeblogApi
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="api"/> class.
        /// </summary>
        public api()
        {
        }


        /// <summary>
        /// Makes a new file to a designated blog using the metaWeblog API
        /// </summary>
        /// <param name="blogid">The blogid.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="file">The file.</param>
        /// <returns>Returns url as a string of a struct.</returns>
        [XmlRpcMethod("metaWeblog.newMediaObject",
            Description = "Makes a new file to a designated blog using the "
                          + "metaWeblog API. Returns url as a string of a struct.")]
        public UrlData newMediaObject(
            string blogid,
            string username,
            string password,
            FileData file)
        {
            return newMediaObjectLogic(blogid, username, password, file);
        }

        #region IRemixWeblogApi Members

        /// <summary>
        /// Gets a summary of all the pages from the blog with the spefied blogId.
        /// </summary>
        /// <param name="blogid">The blogid.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public wpPageSummary[] getPageList(string blogid, string username, string password)
        {
            if (User.validateCredentials(username, password, false))
            {
                ArrayList blogPosts = new ArrayList();
                ArrayList blogPostsObjects = new ArrayList();

                User u = new User(username);
                Channel userChannel = new Channel(u.Id);


                Document rootDoc;
                if (userChannel.StartNode > 0)
                    rootDoc = new Document(userChannel.StartNode);
                else
                    rootDoc = new Document(u.StartNodeId);

                //store children array here because iterating over an Array object is very inneficient.
                var c = rootDoc.Children;
                foreach (Document d in c)
                {
                    int count = 0;
                    blogPosts.AddRange(
                        findBlogPosts(userChannel, d, u.Name, ref count, 999, userChannel.FullTree));
                }

                blogPosts.Sort(new DocumentSortOrderComparer());

                foreach (Object o in blogPosts)
                {
                    Document d = (Document)o;
                    wpPageSummary p = new wpPageSummary();
                    p.dateCreated = d.CreateDateTime;
                    p.page_title = d.Text;
                    p.page_id = d.Id;
                    p.page_parent_id = d.ParentId;

                    blogPostsObjects.Add(p);
                }


                return (wpPageSummary[])blogPostsObjects.ToArray(typeof(wpPageSummary));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a specified number of pages from the blog with the spefied blogId 
        /// </summary>
        /// <param name="blogid">The blogid.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="numberOfItems">The number of pages.</param>
        /// <returns></returns>
        public wpPage[] getPages(string blogid, string username, string password, int numberOfItems)
        {
            if (User.validateCredentials(username, password, false))
            {
                ArrayList blogPosts = new ArrayList();
                ArrayList blogPostsObjects = new ArrayList();

                User u = new User(username);
                Channel userChannel = new Channel(u.Id);


                Document rootDoc;
                if (userChannel.StartNode > 0)
                    rootDoc = new Document(userChannel.StartNode);
                else
                    rootDoc = new Document(u.StartNodeId);

                //store children array here because iterating over an Array object is very inneficient.
                var c = rootDoc.Children;
                foreach (Document d in c)
                {
                    int count = 0;
                    blogPosts.AddRange(
                        findBlogPosts(userChannel, d, u.Name, ref count, numberOfItems, userChannel.FullTree));
                }

                blogPosts.Sort(new DocumentSortOrderComparer());

                foreach (Object o in blogPosts)
                {
                    Document d = (Document)o;
                    wpPage p = new wpPage();
                    p.dateCreated = d.CreateDateTime;
                    p.title = d.Text;
                    p.page_id = d.Id;
                    p.wp_page_parent_id = d.ParentId;
                    p.wp_page_parent_title = d.Parent.Text;
                    p.permalink = library.NiceUrl(d.Id);
                    p.description = d.getProperty(userChannel.FieldDescriptionAlias).Value.ToString();
                    p.link = library.NiceUrl(d.Id);

                    if (userChannel.FieldCategoriesAlias != null && userChannel.FieldCategoriesAlias != "" &&
                        d.getProperty(userChannel.FieldCategoriesAlias) != null &&
                        ((string)d.getProperty(userChannel.FieldCategoriesAlias).Value) != "")
                    {
                        String categories = d.getProperty(userChannel.FieldCategoriesAlias).Value.ToString();
                        char[] splitter = { ',' };
                        String[] categoryIds = categories.Split(splitter);
                        p.categories = categoryIds;
                    }


                    blogPostsObjects.Add(p);
                }


                return (wpPage[])blogPostsObjects.ToArray(typeof(wpPage));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new blog category / tag.
        /// </summary>
        /// <param name="blogid">The blogid.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        public string newCategory(
            string blogid,
            string username,
            string password,
            wpCategory category)
        {
            if (User.validateCredentials(username, password, false))
            {
                Channel userChannel = new Channel(username);
                if (userChannel.FieldCategoriesAlias != null && userChannel.FieldCategoriesAlias != "")
                {
                    // Find the propertytype via the document type
                    ContentType blogPostType = ContentType.GetByAlias(userChannel.DocumentTypeAlias);
                    PropertyType categoryType = blogPostType.getPropertyType(userChannel.FieldCategoriesAlias);
                    interfaces.IUseTags tags = UseTags(categoryType);
                    if (tags != null)
                    {
                        tags.AddTag(category.name);
                    }
                    else
                    {
                        PreValue pv = new PreValue();
                        pv.DataTypeId = categoryType.DataTypeDefinition.Id;
                        pv.Value = category.name;
                        pv.Save();
                    }
                }
            }
            return "";
        }

        #endregion
    }
}