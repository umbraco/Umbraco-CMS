using System.Collections;

namespace Umbraco.Web._Legacy.BusinessLogic
{
    /// <summary>
    /// A collection of tasks.
    /// </summary>
    public class Tasks : CollectionBase
    {
        /// <summary>
        /// Adds the specified new task.
        /// </summary>
        /// <param name="NewTask">The new task.</param>
        public virtual void Add(Task NewTask)
        {
            this.List.Add(NewTask);
        }

        /// <summary>
        /// Gets the <see cref="umbraco.cms.businesslogic.task.Task"/> at the specified index.
        /// </summary>
        /// <value></value>
        public virtual Task this[int Index]
        {
            get { return (Task)this.List[Index]; }
        }
    }
}
