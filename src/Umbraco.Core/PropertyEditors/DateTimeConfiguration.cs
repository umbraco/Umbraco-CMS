namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the datetime value editor.
    /// </summary>
    public class DateTimeConfiguration
    {
        [ConfigurationField("format", "Date format", "textstring", Description = "If left empty then the format is YYYY-MM-DD. (see momentjs.com for supported formats)")]
        public string Format { get; set; }

        public DateTimeConfiguration()
        {
            // different default values
            Format = "YYYY-MM-DD HH:mm:ss";
        }

        [ConfigurationField("offsetTime", "Offset time", "boolean", Description = "When enabled the time displayed will be offset with the server's timezone, this is useful for scenarios like scheduled publishing when an editor is in a different timezone than the hosted server")]
        public bool OffsetTime { get; set; }
    }
}
