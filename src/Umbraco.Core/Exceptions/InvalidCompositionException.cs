using System;

namespace Umbraco.Core.Exceptions
{
    public class InvalidCompositionException : Exception
    {  
        public string ContentTypeAlias { get; set; }

        public string AddedCompositionAlias { get; set; }

        public string PropertyTypeAlias { get; set; }

        public override string Message
        {
            get
            {
                return string.Format(
                        "InvalidCompositionException - ContentType with alias '{0}' was added as a Compsition to ContentType with alias '{1}', " +
                        "but there was a conflict on the PropertyType alias '{2}'. " +
                        "PropertyTypes must have a unique alias across all Compositions in order to compose a valid ContentType Composition.",
                        AddedCompositionAlias, ContentTypeAlias, PropertyTypeAlias);
            }
        }
    }
}