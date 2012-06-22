
using CookComputing.XmlRpc;

namespace CookComputing.Blogger
{
    /// <summary>
    /// Struct representing a blog category
    /// </summary>
    public struct Category
    {
        public string categoryid;
        public string title;
        public string description;
        public string htmlUrl;
        public string rssUrl;
    }

    /// <summary>
    /// Struct representing a blog post
    /// </summary>
    public struct Post
    {
        public System.DateTime dateCreated;
        [XmlRpcMember(
           Description = "Depending on server may be either string or integer. "
           + "Use Convert.ToInt32(userid) to treat as integer or "
           + "Convert.ToString(userid) to treat as string")]

        //Livejournal sometimes drops these
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public object userid;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string postid;
        public string content;
    }

    /// <summary>
    /// Struct containing user information
    /// </summary>
    public struct UserInfo
    {
        public string url;
        public string email;
        public string nickname;
        public string lastname;
        public string firstname;
    }

    /// <summary>
    /// Struct containing Blog information
    /// </summary>
    public struct BlogInfo
    {
        public string blogid;
        public string url;
        public string blogName;
    }

    public interface IBlogger
    {
        [XmlRpcMethod("blogger.deletePost",
           Description = "Deletes a post.")]
        [return: XmlRpcReturnValue(Description = "Always returns true.")]
        bool deletePost(
          string appKey,
          string postid,
          string username,
          string password,
          [XmlRpcParameter(
             Description = "Where applicable, this specifies whether the blog "
             + "should be republished after the post has been deleted.")]
      bool publish);

        [XmlRpcMethod("blogger.editPost",
           Description = "Edits a given post. Optionally, will publish the "
           + "blog after making the edit.")]
        [return: XmlRpcReturnValue(Description = "Always returns true.")]
        object editPost(
          string appKey,
          string postid,
          string username,
          string password,
          string content,
          bool publish);

        [XmlRpcMethod("blogger.getCategories",
           Description = "Returns a list of the categories that you can use "
           + "to log against a post.")]
        Category[] getCategories(
          string blogid,
          string username,
          string password);

        [XmlRpcMethod("blogger.getPost",
           Description = "Returns a single post.")]
        Post getPost(
          string appKey,
          string postid,
          string username,
          string password);

        [XmlRpcMethod("blogger.getRecentPosts",
           Description = "Returns a list of the most recent posts in the system.")]
        Post[] getRecentPosts(
          string appKey,
          string blogid,
          string username,
          string password,
          int numberOfPosts);

        [XmlRpcMethod("blogger.getTemplate",
           Description = "Returns the main or archive index template of "
           + "a given blog.")]
        string getTemplate(
          string appKey,
          string blogid,
          string username,
          string password,
          string templateType);

        [XmlRpcMethod("blogger.getUserInfo",
           Description = "Authenticates a user and returns basic user info "
           + "(name, email, userid, etc.).")]
        UserInfo getUserInfo(
          string appKey,
          string username,
          string password);

        [XmlRpcMethod("blogger.getUsersBlogs",
           Description = "Returns information on all the blogs a given user "
           + "is a member.")]
        BlogInfo[] getUsersBlogs(
          string appKey,
          string username,
          string password);

        [XmlRpcMethod("blogger.newPost",
           Description = "Makes a new post to a designated blog. Optionally, "
           + "will publish the blog after making the post.")]
        [return: XmlRpcReturnValue(Description = "Id of new post")]
        string newPost(
          string appKey,
          string blogid,
          string username,
          string password,
          string content,
          bool publish);

        [XmlRpcMethod("blogger.setTemplate",
           Description = "Edits the main or archive index template of a given blog.")]
        bool setTemplate(
          string appKey,
          string blogid,
          string username,
          string password,
          string template,
          string templateType);
    }
}


