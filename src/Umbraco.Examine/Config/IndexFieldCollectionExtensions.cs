using System.Collections.Generic;

namespace Umbraco.Examine.Config
{
    public static class IndexFieldCollectionExtensions
    {
        public static List<ConfigIndexField> ToList(this IndexFieldCollection indexes)
        {
            List<ConfigIndexField> fields = new List<ConfigIndexField>();
            foreach (ConfigIndexField field in indexes)
                fields.Add(field);
            return fields;
        }
    }
}