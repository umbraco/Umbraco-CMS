﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface ITemplateRepository : IRepositoryQueryable<int, ITemplate>
    {
        ITemplate Get(string alias);

        IEnumerable<ITemplate> GetAll(params string[] aliases);

        IEnumerable<ITemplate> GetChildren(int masterTemplateId);
        IEnumerable<ITemplate> GetChildren(string alias);

        IEnumerable<ITemplate> GetDescendants(int masterTemplateId);
        IEnumerable<ITemplate> GetDescendants(string alias);

        /// <summary>
        /// Returns a template as a template node which can be traversed (parent, children)
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        [Obsolete("Use GetDescendants instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        TemplateNode GetTemplateNode(string alias);

        /// <summary>
        /// Given a template node in a tree, this will find the template node with the given alias if it is found in the hierarchy, otherwise null
        /// </summary>
        /// <param name="anyNode"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        [Obsolete("Use GetDescendants instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        TemplateNode FindTemplateInTree(TemplateNode anyNode, string alias);

        /// <summary>
        /// This checks what the default rendering engine is set in config but then also ensures that there isn't already 
        /// a template that exists in the opposite rendering engine's template folder, then returns the appropriate 
        /// rendering engine to use.
        /// </summary> 
        /// <returns></returns>
        /// <remarks>
        /// The reason this is required is because for example, if you have a master page file already existing under ~/masterpages/Blah.aspx
        /// and then you go to create a template in the tree called Blah and the default rendering engine is MVC, it will create a Blah.cshtml 
        /// empty template in ~/Views. This means every page that is using Blah will go to MVC and render an empty page. 
        /// This is mostly related to installing packages since packages install file templates to the file system and then create the 
        /// templates in business logic. Without this, it could cause the wrong rendering engine to be used for a package.
        /// </remarks>
        RenderingEngine DetermineTemplateRenderingEngine(ITemplate template);

        /// <summary>
        /// Validates a <see cref="ITemplate"/>
        /// </summary>
        /// <param name="template"><see cref="ITemplate"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        bool ValidateTemplate(ITemplate template);

        /// <summary>
        /// Gets the content of a template as a stream.
        /// </summary>
        /// <param name="filepath">The filesystem path to the template.</param>
        /// <returns>The content of the template.</returns>
        Stream GetFileContentStream(string filepath);

        /// <summary>
        /// Sets the content of a template.
        /// </summary>
        /// <param name="filepath">The filesystem path to the template.</param>
        /// <param name="content">The content of the template.</param>
        void SetFileContent(string filepath, Stream content);

        long GetFileSize(string filepath);
    }
}