using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.BusinessLogic {
    /// <summary>
    /// ApplicationBase provides an easy to use base class to install event handlers in umbraco.
    /// Class inhiriting from ApplcationBase are automaticly registered and instantiated by umbraco on application start.
    /// To use, inhirite the ApplicationBase Class and add an empty constructor. 
    /// </summary>
    public abstract class ApplicationBase : umbraco.interfaces.IApplication {
        #region IApplication Members

        /// <summary>
        /// Gets the application alias. By Default it returns the full name of the application class.
        /// The Alias must be unique.
        /// </summary>
        /// <value>The application alias.</value>
        public virtual string Alias{
            get { return GetType().FullName; }
        }

        /// <summary>
        /// Gets the application name. By default it returns the application Classname
        /// </summary>
        /// <value>The name.</value>
        public virtual string Name {
            get { return GetType().Name; }
        }

        /// <summary>
        /// Gets the application icon. For use with application installation which is currently not implemented.
        /// The icon is a path to an image file in the /umbraco/images/tray folder
        /// </summary>
        /// <value>The path to the application icon.</value>
        public virtual string Icon {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the application sortorder. For use with application installation which is currently not implemented.
        /// Sets the sortorder for the Icon in the section tray. By default it returns the value 0
        /// </summary>
        /// <value>The sort order.</value>
        public virtual int SortOrder {
            get { return 0; }
        }

        /// <summary>
        /// Gets the application visibility. For use with application installation which is currently not implemented.
        /// Determines if the application is installed as a visible section in umbraco or just a service running.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        public virtual bool Visible {
            get { return false; }
        }

        /// <summary>
        /// The alias of a application tree to be initialized when section is opened in the backend.
        /// Should only be set if a applcation tree needs to loaded right away.
        /// </summary>
        /// <value>The init tree alias.</value>
        public virtual string InitTreeAlias {
            get { return string.Empty; }
        }

        /// <summary>
        /// Collection of application trees. For use with application installation which is currently not implemented.
        /// Contains the collection of application trees to be installed along with the application itself.
        /// Return null by default.
        /// </summary>
        /// <value>The application trees.</value>
        public virtual List<umbraco.interfaces.ITree> ApplicationTrees {
            get { return null; }
        }

        #endregion
    }
}
