using umbraco.interfaces;

namespace Umbraco.Web.Strategies
{
    /// <summary>
    /// Subscribes to Template Deleted event in order to remove Foreign key reference
    /// from cmsDocument table (Template Id set explicitly on content), and from cmsDocumentType
    /// table where the ContentType references the Template Allowed/Default.
    /// </summary>
    internal class DeletedTemplate : IApplicationStartupHandler
    {
         
    }
}