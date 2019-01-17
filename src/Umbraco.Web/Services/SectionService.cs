using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;
using File = System.IO.File;

namespace Umbraco.Web.Services
{
    internal class SectionService : ISectionService
    {
        private readonly IUserService _userService;
        private readonly BackOfficeSectionCollection _sectionCollection;

        public SectionService(
            IUserService userService,
            BackOfficeSectionCollection sectionCollection)
        {
            _userService = userService;
            _sectionCollection = sectionCollection;
        }
        
        /// <summary>
        /// The cache storage for all applications
        /// </summary>
        public IEnumerable<IBackOfficeSection> GetSections() => _sectionCollection;

        /// <inheritdoc />
        public IEnumerable<IBackOfficeSection> GetAllowedSections(int userId)
        {
            var user = _userService.GetUserById(userId);
            if (user == null)
                throw new InvalidOperationException("No user found with id " + userId);

            return GetSections().Where(x => user.AllowedSections.Contains(x.Alias));
        }

        /// <inheritdoc />
        public IBackOfficeSection GetByAlias(string appAlias) => GetSections().FirstOrDefault(t => t.Alias == appAlias);
        
    }
}
