using System;

namespace Umbraco.Core.Exceptions
{
    public class InvalidCompositionException : Exception
    {
        public InvalidCompositionException(string contentTypeAlias, string addedCompositionAlias, string[] propertyTypeAliass)
        {
            ContentTypeAlias = contentTypeAlias;
            AddedCompositionAlias = addedCompositionAlias;
            PropertyTypeAliases = propertyTypeAliass;
        }

        public InvalidCompositionException(string contentTypeAlias, string[] propertyTypeAliass)
        {
            ContentTypeAlias = contentTypeAlias;
            PropertyTypeAliases = propertyTypeAliass;
        }

        public string ContentTypeAlias { get; private set; }

        public string AddedCompositionAlias { get; private set; }

        public string[] PropertyTypeAliases { get; private set; }

        public override string Message
        {
            get
            {
                return AddedCompositionAlias.IsNullOrWhiteSpace()
                    ? string.Format(
                        "ContentType with alias '{0}' has an invalid composition " +
                        "and there was a conflict on the following PropertyTypes: '{1}'. " +
                        "PropertyTypes must have a unique alias across all Compositions in order to compose a valid ContentType Composition.",
                        ContentTypeAlias, string.Join(", ", PropertyTypeAliases))
                    : string.Format(
                        "ContentType with alias '{0}' was added as a Composition to ContentType with alias '{1}', " +
                        "but there was a conflict on the following PropertyTypes: '{2}'. " +
                        "PropertyTypes must have a unique alias across all Compositions in order to compose a valid ContentType Composition.",
                        AddedCompositionAlias, ContentTypeAlias, string.Join(", ", PropertyTypeAliases));
            }
        }
    }
}