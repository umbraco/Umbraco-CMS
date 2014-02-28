namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a stored pre-value field value
    /// </summary>
    public class PreValue
    {
        public PreValue(int id, string value, int sortOrder)
        {
            Id = id;
            Value = value;       
            SortOrder = sortOrder;
        }

        public PreValue(int id, string value)
        {
            Id = id; 
            Value = value;            
        }

        public PreValue(string value)
        {
            Value = value;
        }

        /// <summary>
        /// The value stored for the pre-value field
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// The database id for the pre-value field value
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The sort order stored for the pre-value field value
        /// </summary>
        public int SortOrder { get; private set; }
    }
}