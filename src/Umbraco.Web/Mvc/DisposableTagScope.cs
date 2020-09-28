using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Umbraco.Core;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Class for creating HTML elements with seperate start and end tags.
    /// </summary>
    /// <seealso cref="Umbraco.Web.Mvc.IDisposableTagScope" />
    public class DisposableTagScope : IDisposableTagScope
    {
        #region Fields

        /// <summary>
        /// The tag builder.
        /// </summary>
        private readonly TagBuilder tagBuilder;

        /// <summary>
        /// Indicates whether the tag is started.
        /// </summary>
        private bool started = false;

        /// <summary>
        /// Indicates whether this instance is disposed.
        /// </summary>
        private bool disposed = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the view context.
        /// </summary>
        /// <value>
        /// The view context.
        /// </value>
        protected ViewContext ViewContext { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableTagScope" /> class.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <exception cref="System.ArgumentNullException">viewContext</exception>
        public DisposableTagScope(ViewContext viewContext, string tagName, object htmlAttributes = null)
        {
            this.ViewContext = viewContext ?? throw new ArgumentNullException(nameof(viewContext));

            var tagBuilder = new TagBuilder(tagName);
            var htmlAttributesDictionary = htmlAttributes as IDictionary<string, object> ?? HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            htmlAttributesDictionary.RemoveAll(kvp => kvp.Value == null);
            tagBuilder.MergeAttributes(htmlAttributesDictionary);

            this.tagBuilder = tagBuilder;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DisposableTagScope" /> class.
        /// </summary>
        ~DisposableTagScope() => this.Dispose(false);

        #endregion

        #region Methods

        /// <summary>
        /// Writes the start tag.
        /// </summary>
        /// <returns>
        /// The current instance as <see cref="IDisposable" />.
        /// </returns>
        public virtual IDisposable Start()
        {
            if (!this.started)
            {
                this.started = true;

                this.WriteStartTag();
            }

            return this;
        }

        /// <summary>
        /// Writes the start tag.
        /// </summary>
        protected void WriteStartTag()
        {
            this.ViewContext.Writer.Write(this.tagBuilder.ToString(TagRenderMode.StartTag));
        }

        /// <summary>
        /// Writes the end tag.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the end tag was written; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool End()
        {
            if (this.started)
            {
                this.started = false;

                this.WriteEndTag();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Writes the end tag.
        /// </summary>
        protected void WriteEndTag()
        {
            this.ViewContext.Writer.Write(this.tagBuilder.ToString(TagRenderMode.EndTag));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;

                if (disposing)
                {
                    while (this.End())
                    {
                        // Ensure all end tags are written
                    }
                }
            }
        }

        #endregion
    }
}
