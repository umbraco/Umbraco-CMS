namespace Umbraco.Cms.Core.Strings
{
    /// <summary>
    /// Represents an HTML-encoded string that should not be encoded again.
    /// </summary>
    public class HtmlEncodedString : IHtmlEncodedString
    {

        private string _htmlString;

        /// <summary>Initializes a new instance of the <see cref="T:System.Web.HtmlString" /> class.</summary>
        /// <param name="value">An HTML-encoded string that should not be encoded again.</param>
        public HtmlEncodedString(string value)
        {
            this._htmlString = value;
        }

        /// <summary>Returns an HTML-encoded string.</summary>
        /// <returns>An HTML-encoded string.</returns>
        public string ToHtmlString()
        {
            return this._htmlString;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return this._htmlString;
        }

    }
}
