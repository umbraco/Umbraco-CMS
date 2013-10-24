namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a stored pre-value field value
    /// </summary>
    public class PreValue
    {
        public PreValue(int id, string value)
        {
            Value = value;
            Id = id;
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
    }
}