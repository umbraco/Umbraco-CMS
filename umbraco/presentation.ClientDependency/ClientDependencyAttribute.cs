using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.presentation.ClientDependency
{
	/// <summary>
	/// This attribute is used for data types that uses client assets like Javascript and CSS for liveediting.
	/// The Live Editing feature in umbraco will look for this attribute and preload all dependencies to the page
	/// to ensure that all client events and assets gets loaded
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ClientDependencyAttribute : Attribute, IClientDependencyFile
	{
        public ClientDependencyAttribute()
        {
            Priority = DefaultPriority;
        }

        /// <summary>
        /// If a priority is not set, the default will be 100.
        /// </summary>
        /// <remarks>
        /// This will generally mean that if a developer doesn't specify a priority it will come after all other dependencies that 
        /// have unless the priority is explicitly set above 100.
        /// </remarks>
        protected const int DefaultPriority = 100;

		/// <summary>
		/// If dependencies have a composite group name specified, the system will combine all dependency
		/// file contents under the one group name and GZIP the output to output cache.
		/// </summary>
		/// <remarks>
		/// This is optional but should be used to decrease the number of requests, save bandwidth and increase
		/// performance on the client side.
		/// Though both javascript and css files may have the same group name specified, they will be treated seperately.
		/// </remarks>
		public string CompositeGroupName { get; set; }

		/// <summary>
		/// Gets or sets the priority.
		/// </summary>
		/// <value>The priority.</value>
		public int Priority { get; set; }

		/// <summary>
		/// Gets or sets the file path.
		/// </summary>
		/// <value>The file path.</value>
		public string FilePath { get; set; }

		/// <summary>
		/// The path alias to be pre-pended to the file path if specified.
		/// The alias is specified in in the ClientDependencyHelper constructor.
		/// If the alias specified does not exist in the ClientDependencyHelper
		/// path collection, an exception is thrown.
		/// </summary>
		public string PathNameAlias { get; set; }

		/// <summary>
		/// Gets or sets the type of the dependency.
		/// </summary>
		/// <value>The type of the dependency.</value>
		public ClientDependencyType DependencyType { get; set; }

		/// <summary>
		/// Gets or sets the name of an optional javascript method that should be called on load.
		/// </summary>
		/// <value>The name of the method.</value>
		public string InvokeJavascriptMethodOnLoad { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientDependencyAttribute"/> class.
		/// </summary>
		/// <param name="priority">The priority.</param>
		/// <param name="dependencyType">Type of the dependency.</param>
		/// <param name="fullFilePath">The file path to the dependency.</param>
		public ClientDependencyAttribute(int priority, ClientDependencyType dependencyType, string fullFilePath)
			: this(priority, dependencyType, fullFilePath, string.Empty, string.Empty)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientDependencyAttribute"/> class.
		/// </summary>
		/// <param name="priority">The priority.</param>
		/// <param name="dependencyType">Type of the dependency.</param>
		/// <param name="filePath">The file path to the dependency.</param>
		/// <param name="pathName">
		/// If set, this will prepend the 'Path' found in the ClientDependencyPathCollection with the pathName specified.
		/// If the pathName specified is not found in the collection, an exception will be thrown.
		/// </param>
		public ClientDependencyAttribute(int priority, ClientDependencyType dependencyType, string fileName, string pathNameAlias)
			: this(priority, dependencyType, fileName, pathNameAlias, string.Empty)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientDependencyAttribute"/> class.
		/// </summary>
		/// <param name="priority">The priority.</param>
		/// <param name="dependencyType">Type of the dependency.</param>
		/// <param name="filePath">The file path to the dependency.</param>
		/// <param name="appendUmbracoPath">if set to <c>true</c> the current umbraco path will be prefixed to the filePath.</param>
		/// <param name="invokeJavascriptMethodOnLoad">The name of the Javascript method to invoke when the dependency is loaded.</param>
        public ClientDependencyAttribute(int priority, ClientDependencyType dependencyType, string fileName, string pathNameAlias, string invokeJavascriptMethodOnLoad)
            : this(priority, dependencyType, fileName, pathNameAlias, invokeJavascriptMethodOnLoad, string.Empty)
        { }

        public ClientDependencyAttribute(int priority, ClientDependencyType dependencyType, string fileName, string pathNameAlias, string invokeJavascriptMethodOnLoad, string compositeGroupName)
        {
            if (String.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            Priority = priority;


            FilePath = fileName;
            PathNameAlias = pathNameAlias;
            CompositeGroupName = compositeGroupName;

            DependencyType = dependencyType;
            InvokeJavascriptMethodOnLoad = invokeJavascriptMethodOnLoad ?? string.Empty;
        }
	}

	
}
