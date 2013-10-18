using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Tests.PublishedContent.StronglyTypedModels
{
    /// <summary>
    /// Represents a Textpage which acts as the strongly typed model for a Doc Type 
    /// with alias "Textpage" and "Subpage" as the allowed child type.
    /// 
    /// The basic properties are resolved by convention using the Resolve-Type-PropertyTypeAlias
    /// convention available through the base class' protected Resolve-method and Property-delegate.
    /// 
    /// The Textpage allows the use of Subpage and Textpage as child doc types, which are exposed as a 
    /// collection using the Children-Type-ContentTypeAlias convention available through the
    /// base class' protected Children-method and ContentTypeAlias-delegate.
    /// </summary>
    /// <remarks>
    /// This code can easily be generated using simple conventions for the types and names
    /// of the properties that this type of strongly typed model exposes.
    /// </remarks>
    public class Textpage : TypedModelBase
    {
        public Textpage(IPublishedContent publishedContent) : base(publishedContent)
        {
        }

        public string Title { get { return Resolve<string>(Property()); } }

        public string BodyText { get { return Resolve<string>(Property()); } }

        public string AuthorName { get { return Resolve<string>(Property()); } }

        public DateTime Date { get { return Resolve<DateTime>(Property()); } }

        public Textpage Parent
        {
            get
            {
                return Parent<Textpage>();
            }
        }

        public IEnumerable<Textpage> Textpages
        {
            get
            {
                return Children<Textpage>(ContentTypeAlias());
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