using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Tests.PublishedContent.StronglyTypedModels
{
    /// <summary>
    /// Represents a Subpage which acts as the strongly typed model for a Doc Type 
    /// with alias "Subpage" and "Subpage" as the allowed child type.
    /// 
    /// Similar to the Textpage this model could also be generated, but it could also
    /// act as a Code-First model by using the attributes shown on the various properties,
    /// which decorate the model with information about the Document Type, its 
    /// Property Groups and Property Types.
    /// </summary>
    public class Subpage : TypedModelBase
    {
        public Subpage(IPublishedContent publishedContent) : base(publishedContent)
        {
        }

        public string Title { get { return Resolve<string>(Property()); } }

        public string BodyText { get { return Resolve<string>(Property()); } }

        public Textpage Parent
        {
            get
            {
                return Parent<Textpage>();
            }
        }

        public IEnumerable<Subpage> Subpages
        {
            get
            {
                return Children<Subpage>(ContentTypeAlias());
            }
        }
    }
}