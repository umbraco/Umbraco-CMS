namespace Umbraco.Cms.Core.Models.Preview
{
    /// <summary>
    ///     A model representing the accessiblity checker configuration.
    /// </summary>
    public class SA11YAccessiblityChecker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SA11YAccessiblityChecker"/> class.
        /// </summary>
        public SA11YAccessiblityChecker() 
        {
            LanguageJS = "en";
        }
        /// <summary>
        ///     Gets the Language JS file to use for the accessiblity checker.
        /// </summary>
        /// <value>
        ///     The LanguageJS.
        /// </value>
        public string LanguageJS { get; set; }

    }
}
