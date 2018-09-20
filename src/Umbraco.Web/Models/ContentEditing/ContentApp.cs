using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Defines a "Content App" which are editor extensions
    /// </summary>
    [DataContract(Name = "app", Namespace = "")]
    public class ContentApp
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        [DataMember(Name = "icon")]
        public string Icon { get; set; }

        [DataMember(Name = "view")]
        public string View { get; set; }

        /// <summary>
        /// The view model specific to this app
        /// </summary>
        [DataMember(Name = "viewModel")]
        public object ViewModel { get; set; }

        /// <summary>
        /// Normally reserved for Angular to deal with but in some cases this can be set on the server side
        /// </summary>
        [DataMember(Name = "active")]
        public bool Active { get; set; }
    }
}

