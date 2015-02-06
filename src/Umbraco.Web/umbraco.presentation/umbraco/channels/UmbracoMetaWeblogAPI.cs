using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Web;
using CookComputing.Blogger;
using CookComputing.MetaWeblog;
using CookComputing.XmlRpc;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Logging;
using Umbraco.Core.Security;
using umbraco.presentation.channels.businesslogic;
using Post = CookComputing.MetaWeblog.Post;

using System.Collections.Generic;
using System.Web.Security;
using Umbraco.Core.IO;
using Umbraco.Core;

namespace umbraco.presentation.channels
{
    public abstract class UmbracoMetaWeblogAPI : XmlRpcService, IMetaWeblog
    {
        internal readonly MediaFileSystem _fs;

        protected UmbracoMetaWeblogAPI()
        {
            _fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
        }

        [XmlRpcMethod("blogger.deletePost",
            Description = "Deletes a post.")]
        [return: XmlRpcReturnValue(Description = "Always returns true.")]
        public bool deletePost(
            string appKey,
            string postid,
            string username,
            string password,
            [XmlRpcParameter(
                Description = "Where applicable, this specifies whether the blog "
                              + "should be republished after the post has been deleted.")] bool publish)
        {
            if (ValidateUser(username, password))
            {
                Channel userChannel = new Channel(username);
                new Document(int.Parse(postid))
                    .delete();
                return true;
            }
            return false;
        }

        public object editPost(
            string postid,
            string username,
            string password,
            Post post,
            bool publish)
        {
            if (ValidateUser(username, password))
            {
                Channel userChannel = new Channel(username);
                Document doc = new Document(Convert.ToInt32(postid));


                doc.Text = HttpContext.Current.Server.HtmlDecode(post.title);

                // Excerpt
                if (userChannel.FieldExcerptAlias != null && userChannel.FieldExcerptAlias != "")
                    doc.getProperty(userChannel.FieldExcerptAlias).Value = RemoveLeftUrl(post.mt_excerpt);

                
                if (UmbracoConfig.For.UmbracoSettings().Content.TidyEditorContent)
                    doc.getProperty(userChannel.FieldDescriptionAlias).Value = library.Tidy(RemoveLeftUrl(post.description), false);
                else
                    doc.getProperty(userChannel.FieldDescriptionAlias).Value = RemoveLeftUrl(post.description);

                UpdateCategories(doc, post, userChannel);


                if (publish)
                {
                    doc.SaveAndPublish(new User(username));
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void UpdateCategories(Document doc, Post post, Channel userChannel)
        {
            if (userChannel.FieldCategoriesAlias != null && userChannel.FieldCategoriesAlias != "")
            {
                ContentType blogPostType = ContentType.GetByAlias(userChannel.DocumentTypeAlias);
                PropertyType categoryType = blogPostType.getPropertyType(userChannel.FieldCategoriesAlias);

                String[] categories = post.categories;
                string categoryValue = "";
                interfaces.IUseTags tags = UseTags(categoryType);
                if (tags != null)
                {
                    tags.RemoveTagsFromNode(doc.Id);
                    for (int i = 0; i < categories.Length; i++)
                    {
                        tags.AddTagToNode(doc.Id, categories[i]);
                    }
                    //If the IUseTags provider manually set the property value to something on the IData interface then we should persist this
                    //code commented as for some reason, even though the IUseTags control is setting IData.Value it is null here
                    //could be a cache issue, or maybe it's a different instance of the IData or something, rather odd
                    //doc.getProperty(userChannel.FieldCategoriesAlias).Value = categoryType.DataTypeDefinition.DataType.Data.Value;

                    //Instead, set the document property to CSV of the tags - this WILL break custom editors for tags which don't adhere to the
                    //pseudo standard that the .Value of the property contains CSV tags. 
                    doc.getProperty(userChannel.FieldCategoriesAlias).Value = string.Join(",", categories);
                }
                else
                {
                    for (int i = 0; i < categories.Length; i++)
                    {
                        PreValue pv = new PreValue(categoryType.DataTypeDefinition.Id, categories[i]);
                        categoryValue += pv.Id + ",";
                    }
                    if (categoryValue.Length > 0)
                        categoryValue = categoryValue.Substring(0, categoryValue.Length - 1);

                    doc.getProperty(userChannel.FieldCategoriesAlias).Value = categoryValue;
                }
            }
        }

        public CategoryInfo[] getCategories(
            string blogid,
            string username,
            string password)
        {
            if (ValidateUser(username, password))
            {
                Channel userChannel = new Channel(username);
                if (userChannel.FieldCategoriesAlias != null && userChannel.FieldCategoriesAlias != "")
                {
                    // Find the propertytype via the document type
                    ContentType blogPostType = ContentType.GetByAlias(userChannel.DocumentTypeAlias);
                    PropertyType categoryType = blogPostType.getPropertyType(userChannel.FieldCategoriesAlias);

                    // check if the datatype uses tags or prevalues
                    CategoryInfo[] returnedCategories = null;
                    interfaces.IUseTags tags = UseTags(categoryType);
                    if (tags != null)
                    {
                        List<interfaces.ITag> alltags = tags.GetAllTags();
                        if (alltags != null)
                        {
                            returnedCategories = new CategoryInfo[alltags.Count];
                            int counter = 0;
                            foreach (interfaces.ITag t in alltags)
                            {
                                CategoryInfo ci = new CategoryInfo();
                                ci.title = t.TagCaption;
                                ci.categoryid = t.Id.ToString();
                                ci.description = "";
                                ci.rssUrl = "";
                                ci.htmlUrl = "";
                                returnedCategories[counter] = ci;
                                counter++;
                            }
                        }
                        else
                        {
                            returnedCategories = new CategoryInfo[0];
                        }
                    }
                    else
                    {
                        SortedList categories = PreValues.GetPreValues(categoryType.DataTypeDefinition.Id);
                        returnedCategories = new CategoryInfo[categories.Count];
                        IDictionaryEnumerator ide = categories.GetEnumerator();
                        int counter = 0;
                        while (ide.MoveNext())
                        {
                            PreValue category = (PreValue)ide.Value;
                            CategoryInfo ci = new CategoryInfo();
                            ci.title = category.Value;
                            ci.categoryid = category.Id.ToString();
                            ci.description = "";
                            ci.rssUrl = "";
                            ci.htmlUrl = "";
                            returnedCategories[counter] = ci;
                            counter++;
                        }
                    }

                    return returnedCategories;
                }
            }

            throw new ArgumentException("Categories doesn't work for this channel, they might not have been activated. Contact your umbraco administrator.");
        }

        public static interfaces.IUseTags UseTags(PropertyType categoryType)
        {
            if (typeof(interfaces.IUseTags).IsAssignableFrom(categoryType.DataTypeDefinition.DataType.DataEditor.GetType()))
            {
                interfaces.IUseTags tags = (interfaces.IUseTags)categoryType.DataTypeDefinition.DataType.DataEditor as interfaces.IUseTags;
                return tags;
            }
            return null;
        }

        public Post getPost(
            string postid,
            string username,
            string password)
        {
            if (ValidateUser(username, password))
            {
                Channel userChannel = new Channel(username);
                Document d = new Document(int.Parse(postid));
                Post p = new Post();
                p.title = d.Text;
                p.description = d.getProperty(userChannel.FieldDescriptionAlias).Value.ToString();

                // Excerpt
                if (userChannel.FieldExcerptAlias != null && userChannel.FieldExcerptAlias != "")
                    p.mt_excerpt = d.getProperty(userChannel.FieldExcerptAlias).Value.ToString();

                // Categories
                if (userChannel.FieldCategoriesAlias != null && userChannel.FieldCategoriesAlias != "" &&
                    d.getProperty(userChannel.FieldCategoriesAlias) != null &&
                    d.getProperty(userChannel.FieldCategoriesAlias).Value != null &&
                    d.getProperty(userChannel.FieldCategoriesAlias).Value.ToString() != "")
                {
                    String categories = d.getProperty(userChannel.FieldCategoriesAlias).Value.ToString();
                    char[] splitter = { ',' };
                    String[] categoryIds = categories.Split(splitter);
                    p.categories = categoryIds;
                }

                p.postid = postid;
                p.permalink = library.NiceUrl(d.Id);
                p.dateCreated = d.CreateDateTime;
                p.link = p.permalink;
                return p;
            }
            else
                throw new ArgumentException(string.Format("Error retriving post with id: '{0}'", postid));
        }

        public Post[] getRecentPosts(
            string blogid,
            string username,
            string password,
            int numberOfPosts)
        {
            if (ValidateUser(username, password))
            {
                ArrayList blogPosts = new ArrayList();
                ArrayList blogPostsObjects = new ArrayList();

                User u = new User(username);
                Channel userChannel = new Channel(u.Id);


                Document rootDoc;
                if (userChannel.StartNode > 0)
                    rootDoc = new Document(userChannel.StartNode);
                else
                {
                    if (u.StartNodeId == -1)
                    {
                        rootDoc = Document.GetRootDocuments()[0];
                    }
                    else
                    {
                        rootDoc = new Document(u.StartNodeId);
                    }
                }

                //store children array here because iterating over an Array object is very inneficient.
                var c = rootDoc.Children;
                foreach (Document d in c)
                {
                    int count = 0;
                    blogPosts.AddRange(
                        findBlogPosts(userChannel, d, u.Name, ref count, numberOfPosts, userChannel.FullTree));
                }

                blogPosts.Sort(new DocumentSortOrderComparer());

                foreach (Object o in blogPosts)
                {
                    Document d = (Document)o;
                    Post p = new Post();
                    p.dateCreated = d.CreateDateTime;
                    p.userid = username;
                    p.title = d.Text;
                    p.permalink = library.NiceUrl(d.Id);
                    p.description = d.getProperty(userChannel.FieldDescriptionAlias).Value.ToString();
                    p.link = library.NiceUrl(d.Id);
                    p.postid = d.Id.ToString();

                    if (userChannel.FieldCategoriesAlias != null && userChannel.FieldCategoriesAlias != "" &&
                        d.getProperty(userChannel.FieldCategoriesAlias) != null &&
                        d.getProperty(userChannel.FieldCategoriesAlias).Value != null &&
                        d.getProperty(userChannel.FieldCategoriesAlias).Value.ToString() != "")
                    {
                        String categories = d.getProperty(userChannel.FieldCategoriesAlias).Value.ToString();
                        char[] splitter = { ',' };
                        String[] categoryIds = categories.Split(splitter);
                        p.categories = categoryIds;
                    }

                    // Excerpt
                    if (userChannel.FieldExcerptAlias != null && userChannel.FieldExcerptAlias != "")
                        p.mt_excerpt = d.getProperty(userChannel.FieldExcerptAlias).Value.ToString();


                    blogPostsObjects.Add(p);
                }


                return (Post[])blogPostsObjects.ToArray(typeof(Post));
            }
            else
            {
                return null;
            }
        }

        protected ArrayList findBlogPosts(Channel userChannel, Document d, String userName, ref int count, int max,
                                          bool fullTree)
        {
            ArrayList list = new ArrayList();

            ContentType ct = d.ContentType;

            if (ct.Alias.Equals(userChannel.DocumentTypeAlias) &
                (count < max))
            {
                list.Add(d);
                count = count + 1;
            }

            if (d.Children != null && d.Children.Length > 0 && fullTree)
            {
                //store children array here because iterating over an Array object is very inneficient.
                var c = d.Children;
                foreach (Document child in c)
                {
                    if (count < max)
                    {
                        list.AddRange(findBlogPosts(userChannel, child, userName, ref count, max, true));
                    }
                }
            }
            return list;
        }

        public string newPost(
            string blogid,
            string username,
            string password,
            Post post,
            bool publish)
        {
            if (ValidateUser(username, password))
            {
                Channel userChannel = new Channel(username);
                User u = new User(username);
                Document doc =
                    Document.MakeNew(HttpContext.Current.Server.HtmlDecode(post.title),
                                     DocumentType.GetByAlias(userChannel.DocumentTypeAlias), u,
                                     userChannel.StartNode);


                // Excerpt
                if (userChannel.FieldExcerptAlias != null && userChannel.FieldExcerptAlias != "")
                    doc.getProperty(userChannel.FieldExcerptAlias).Value = RemoveLeftUrl(post.mt_excerpt);


                // Description
                if (UmbracoConfig.For.UmbracoSettings().Content.TidyEditorContent)
                    doc.getProperty(userChannel.FieldDescriptionAlias).Value = library.Tidy(RemoveLeftUrl(post.description), false);
                else
                    doc.getProperty(userChannel.FieldDescriptionAlias).Value = RemoveLeftUrl(post.description);

                // Categories
                UpdateCategories(doc, post, userChannel);

                // check release date
                if (post.dateCreated.Year > 0001)
                {
                    publish = false;
                    doc.ReleaseDate = post.dateCreated;
                }

                if (publish)
                {
                    doc.SaveAndPublish(new User(username));
                }
                return doc.Id.ToString();
            }
            else
                throw new ArgumentException("Error creating post");
        }

        protected MediaObjectInfo newMediaObjectLogicForWord(
            string blogid,
            string username,
            string password,
            FileData file)
        {
            UrlData ud = newMediaObjectLogic(blogid, username, password, file);
            MediaObjectInfo moi = new MediaObjectInfo();
            moi.url = ud.url;
            return moi;
        }
        protected UrlData newMediaObjectLogic(
            string blogid,
            string username,
            string password,
            FileData file)
        {
            if (ValidateUser(username, password))
            {
                User u = new User(username);
                Channel userChannel = new Channel(username);
                UrlData fileUrl = new UrlData();
                if (userChannel.ImageSupport)
                {
                    Media rootNode;
                    if (userChannel.MediaFolder > 0)
                        rootNode = new Media(userChannel.MediaFolder);
                    else
                        rootNode = new Media(u.StartMediaId);

                    // Create new media
                    Media m = Media.MakeNew(file.name, MediaType.GetByAlias(userChannel.MediaTypeAlias), u, rootNode.Id);

                    Property fileObject = m.getProperty(userChannel.MediaTypeFileProperty);
                    
                    var filename = file.name.Replace("/", "_");
                    var relativeFilePath = _fs.GetRelativePath(fileObject.Id, filename);

                    fileObject.Value = _fs.GetUrl(relativeFilePath);
                    fileUrl.url = fileObject.Value.ToString();

                    if (!fileUrl.url.StartsWith("http"))
                    {
                        var protocol = GlobalSettings.UseSSL ? "https" : "http";
                        fileUrl.url = protocol + "://" + HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + fileUrl.url;
                    }

                    _fs.AddFile(relativeFilePath, new MemoryStream(file.bits));

                    // Try updating standard file values
                    try
                    {
                        string orgExt = "";
                        // Size
                        if (m.getProperty(Constants.Conventions.Media.Bytes) != null)
                            m.getProperty(Constants.Conventions.Media.Bytes).Value = file.bits.Length;
                        // Extension
                        if (m.getProperty(Constants.Conventions.Media.Extension) != null)
                        {
                            orgExt =
                                ((string)
                                 file.name.Substring(file.name.LastIndexOf(".") + 1,
                                                     file.name.Length - file.name.LastIndexOf(".") - 1));
                            m.getProperty(Constants.Conventions.Media.Extension).Value = orgExt.ToLower();
                        }
                        // Width and Height
                        // Check if image and then get sizes, make thumb and update database
                        if (m.getProperty(Constants.Conventions.Media.Width) != null && m.getProperty(Constants.Conventions.Media.Height) != null &&
                            ",jpeg,jpg,gif,bmp,png,tiff,tif,".IndexOf("," + orgExt.ToLower() + ",") > 0)
                        {
                            int fileWidth;
                            int fileHeight;

                            using (var stream = _fs.OpenFile(relativeFilePath))
                            {
                                Image image = Image.FromStream(stream);
                                fileWidth = image.Width;
                                fileHeight = image.Height;
                                stream.Close();
                                try
                                {
                                    m.getProperty(Constants.Conventions.Media.Width).Value = fileWidth.ToString();
                                    m.getProperty(Constants.Conventions.Media.Height).Value = fileHeight.ToString();
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Error<UmbracoMetaWeblogAPI>("An error occurred reading the media stream", ex);
                                }    
                            }

                            
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<UmbracoMetaWeblogAPI>("An error occurred in newMediaObjectLogic", ex);
                    }

                    return fileUrl;
                }
                else
                    throw new ArgumentException(
                        "Image Support is turned off in this channel. Modify channel settings in umbraco to enable image support.");
            }
            return new UrlData();
        }

        private static bool ValidateUser(string username, string password)
        {
            var provider = MembershipProviderExtensions.GetUsersMembershipProvider();

            return provider.ValidateUser(username, password);
        }

        [XmlRpcMethod("blogger.getUsersBlogs",
            Description = "Returns information on all the blogs a given user "
                          + "is a member.")]
        public BlogInfo[] getUsersBlogs(
            string appKey,
            string username,
            string password)
        {
            if (ValidateUser(username, password))
            {
                BlogInfo[] blogs = new BlogInfo[1];
                User u = new User(username);
                Channel userChannel = new Channel(u.Id);
                Document rootDoc;
                if (userChannel.StartNode > 0)
                    rootDoc = new Document(userChannel.StartNode);
                else
                    rootDoc = new Document(u.StartNodeId);

                BlogInfo bInfo = new BlogInfo();
                bInfo.blogName = userChannel.Name;
                bInfo.blogid = rootDoc.Id.ToString();
                bInfo.url = library.NiceUrlWithDomain(rootDoc.Id, true);
                blogs[0] = bInfo;

                return blogs;
            }

            throw new ArgumentException(string.Format("No data found for user with username: '{0}'", username));
        }

        private static string RemoveLeftUrl(string text)
        {
            return
                text.Replace(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority), "");
        }
    }
}