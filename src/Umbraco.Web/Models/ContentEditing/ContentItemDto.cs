using Newtonsoft.Json;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a content item from the database including all of the required data that we need to work with such as data type data
    /// </summary>
    internal class ContentItemDto<TPersisted> : ContentItemBasic<ContentPropertyDto, TPersisted> 
        where TPersisted : IContentBase
    {
        
    }
}