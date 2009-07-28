using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.cms.businesslogic.datatype
{
	///// <summary>
	///// This attribute is used for data types that uses client assets like Javascript and CSS for liveediting.
	///// The Live Editing feature in umbraco will look for this attribute and preload all dependencies to the page
	///// to ensure that all client events and assets gets loaded
	///// </summary>
	//[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	//public class ClientDependencyAttribute : Attribute
	//{
	//    /// <summary>
	//    /// Gets or sets the priority.
	//    /// </summary>
	//    /// <value>The priority.</value>
	//    public int Priority { get; set; }

	//    /// <summary>
	//    /// Gets or sets the file path.
	//    /// </summary>
	//    /// <value>The file path.</value>
	//    public string FilePath { get; set; }

	//    /// <summary>
	//    /// Gets or sets the type of the dependency.
	//    /// </summary>
	//    /// <value>The type of the dependency.</value>
	//    public ClientDependencyType DependencyType { get; set; }

	//    /// <summary>
	//    /// Gets or sets the name of an optional javascript method that should be called on load.
	//    /// </summary>
	//    /// <value>The name of the method.</value>
	//    public string InvokeJavascriptMethodOnLoad { get; set; }

	//    /// <summary>
	//    /// Initializes a new instance of the <see cref="ClientDependencyAttribute"/> class.
	//    /// </summary>
	//    /// <param name="priority">The priority.</param>
	//    /// <param name="dependencyType">Type of the dependency.</param>
	//    /// <param name="filePath">The file path to the dependency.</param>
	//    public ClientDependencyAttribute(int priority, ClientDependencyType dependencyType, string filePath)
	//        : this(priority, dependencyType, filePath, false, string.Empty)
	//    { }

	//    /// <summary>
	//    /// Initializes a new instance of the <see cref="ClientDependencyAttribute"/> class.
	//    /// </summary>
	//    /// <param name="priority">The priority.</param>
	//    /// <param name="dependencyType">Type of the dependency.</param>
	//    /// <param name="filePath">The file path to the dependency.</param>
	//    /// <param name="invokeJavascriptMethodOnLoad">The name of the Javascript method to invoke when the dependency is loaded.</param>
	//    public ClientDependencyAttribute(int priority, ClientDependencyType dependencyType, string filePath, string invokeJavascriptMethodOnLoad)
	//        : this(priority, dependencyType, filePath, false, invokeJavascriptMethodOnLoad)
	//    { }

	//    /// <summary>
	//    /// Initializes a new instance of the <see cref="ClientDependencyAttribute"/> class.
	//    /// </summary>
	//    /// <param name="priority">The priority.</param>
	//    /// <param name="dependencyType">Type of the dependency.</param>
	//    /// <param name="filePath">The file path to the dependency.</param>
	//    /// <param name="appendUmbracoPath">if set to <c>true</c> the current umbraco path will be prefixed to the filePath.</param>
	//    public ClientDependencyAttribute(int priority, ClientDependencyType dependencyType, string filePath, bool appendUmbracoPath)
	//        : this(priority, dependencyType, filePath, appendUmbracoPath, String.Empty)
	//    { }

	//    /// <summary>
	//    /// Initializes a new instance of the <see cref="ClientDependencyAttribute"/> class.
	//    /// </summary>
	//    /// <param name="priority">The priority.</param>
	//    /// <param name="dependencyType">Type of the dependency.</param>
	//    /// <param name="filePath">The file path to the dependency.</param>
	//    /// <param name="appendUmbracoPath">if set to <c>true</c> the current umbraco path will be prefixed to the filePath.</param>
	//    /// <param name="invokeJavascriptMethodOnLoad">The name of the Javascript method to invoke when the dependency is loaded.</param>
	//    public ClientDependencyAttribute(int priority, ClientDependencyType dependencyType, string filePath, bool appendUmbracoPath, string invokeJavascriptMethodOnLoad)
	//    {
	//        if (String.IsNullOrEmpty(filePath))
	//            throw new ArgumentException("filePath");

	//        Priority = priority;
	//        FilePath = appendUmbracoPath ? GlobalSettings.Path + "/" + filePath : filePath;
	//        DependencyType = dependencyType;
	//        InvokeJavascriptMethodOnLoad = invokeJavascriptMethodOnLoad ?? String.Empty;
	//    }
	//}

	///// <summary>
	///// The type of client file
	///// </summary>
	//public enum ClientDependencyType
	//{
	//    Javascript, Css
	//}
}
