using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Infrastructure.WebAssets
{
    /// <summary>
    /// A notification for when server variables are parsing
    /// </summary>
    public class ServerVariablesParsing : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerVariablesParsing"/> class.
        /// </summary>
        public ServerVariablesParsing(IDictionary<string, object> serverVariables) => ServerVariables = serverVariables;

        /// <summary>
        /// Gets a mutable dictionary of server variables
        /// </summary>
        public IDictionary<string, object> ServerVariables { get; }
    }
}
