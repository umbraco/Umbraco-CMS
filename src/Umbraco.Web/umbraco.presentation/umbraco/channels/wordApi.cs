using CookComputing.MetaWeblog;
using CookComputing.XmlRpc;

namespace umbraco.presentation.channels
{
    /// <summary>
    /// Summary description for Test.
    /// </summary>
    [XmlRpcService(
        Name = "umbraco metablog test",
        Description = "For editing umbraco data from external clients",
        AutoDocumentation = true)]
    public class wordApi : UmbracoMetaWeblogAPI
    {
        public wordApi()
        {
        }

        [XmlRpcMethod("metaWeblog.newMediaObject",
      Description = "Makes a new file to a designated blog using the "
      + "metaWeblog API. Returns url as a string of a struct.")]
        public MediaObjectInfo newMediaObject(
            string blogid,
            string username,
            string password,
            FileData file)
        {
            return newMediaObjectLogicForWord(blogid.ToString(), username, password, file);
        }

    }
}