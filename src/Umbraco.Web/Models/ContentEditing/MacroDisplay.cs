﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// The macro display model
    /// </summary>
    [DataContract(Name = "dictionary", Namespace = "")]
    public class MacroDisplay : EntityBasic, INotificationModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MacroDisplay"/> class.
        /// </summary>
        public MacroDisplay()
        {
            this.Notifications = new List<Notification>();
            this.Parameters = new List<MacroParameterDisplay>();
        }

        /// <inheritdoc />
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the macro can be used in a rich text editor.
        /// </summary>
        [DataMember(Name = "useInEditor")]
        public bool UseInEditor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the macro should be rendered a rich text editor.
        /// </summary>
        [DataMember(Name = "renderInEditor")]
        public bool RenderInEditor { get; set; }

        /// <summary>
        /// Gets or sets the cache period.
        /// </summary>
        [DataMember(Name = "cachePeriod")]
        public int CachePeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the macro should be cached by page
        /// </summary>
        [DataMember(Name = "cacheByPage")]
        public bool CacheByPage { get; set; }

        /// <summary> 
        /// Gets or sets a value indicating whether the macro should be cached by user
        /// </summary>
        [DataMember(Name = "cacheByUser")]
        public bool CacheByUser { get; set; }

        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        [DataMember(Name = "view")]
        public string View { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        [DataMember(Name = "parameters")]
        public IEnumerable<MacroParameterDisplay> Parameters { get; set; }
    }
}
