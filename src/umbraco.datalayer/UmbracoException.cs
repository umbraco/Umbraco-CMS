/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Diagnostics;
using System.Reflection;

namespace umbraco
{
    /// <summary>
    /// Represents an exception that is generated in an Umbraco module.
    /// </summary>
    /// <remarks>This should be moved out of the data layer for general use.</remarks>
    public class UmbracoException : Exception
    {
        #region Private Fields
        
        /// <summary>The Umbraco component that generated the exception.</summary>
        private readonly UmbracoComponent m_Component; 

        #endregion

        #region Public Properties
        
        /// <summary>
        /// Gets the Umbraco component that generated the exception.
        /// </summary>
        /// <value>The component.</value>
        public UmbracoComponent Component
        {
            get { return m_Component; }
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception.</value>
        public override string Message
        {
            get
            {
                return String.Format("Umbraco Exception ({0}): {1}", Component, base.Message);
            }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public UmbracoException(string message)
            : this(message, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public UmbracoException(string message, Exception innerException)
            : base(message, innerException)
        {
            // get the calling assembly
            Assembly assembly = new StackFrame(1).GetMethod().ReflectedType.Assembly;
            string assemblyName = assembly.FullName.Split(",".ToCharArray())[0];
            // try to determine the component
            try
            {
                string componentName = assemblyName.ToLower().Replace("umbraco.", String.Empty);
                m_Component = (UmbracoComponent)Enum.Parse(typeof(UmbracoComponent), componentName, true);
            }
            // do nothing if parsing fails, default value is UmbracoComponent.External
            catch (ArgumentException) { }
        }

        #endregion
    }

    /// <summary>
    /// Enum of all Umbraco components
    /// </summary>
    public enum UmbracoComponent
    {
        /// <summary>Unknown component</summary>
        External,
        /// <summary>Business logic component</summary>
        BusinessLogic,
        /// <summary>CMS component</summary>
        CMS,
        /// <summary>Controls component</summary>
        Controls,
        /// <summary>Data layer component</summary>
        DataLayer,
        /// <summary>Editor controls component</summary>
        EditorControls,
        /// <summary>Presentation component</summary>
        Presentation,
        /// <summary>Providers component</summary>
        Providers
    }
}
