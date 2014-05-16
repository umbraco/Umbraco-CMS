namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a stored pre-value field value
    /// </summary>
    public class PreValue : IDeepCloneable
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
        public string Value { get; set; }

        /// <summary>
        /// The database id for the pre-value field value
        /// </summary>
        public int Id { get; private set; }

        public virtual object DeepClone()
        {
            //Memberwise clone on PreValue will work since it doesn't have any deep elements
            var clone = (PreValue)MemberwiseClone();            
            return clone;
        }
    }
}