// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelatedLinks.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//   Defines the RelatedLinks type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

﻿using System.Collections;
using System.Collections.Generic;
﻿using System.ComponentModel;
﻿using System.Linq;

namespace Umbraco.Web.Models
{
    [TypeConverter(typeof(RelatedLinksTypeConverter))]
    public class RelatedLinks : IEnumerable<RelatedLink>
    {
        private readonly string _propertyData;

        private readonly List<RelatedLink> _relatedLinks;

        public RelatedLinks(List<RelatedLink> relatedLinks, string propertyData)
        {
            _relatedLinks = relatedLinks;
            _propertyData = propertyData;
        }

        /// <summary>
        /// Gets the property data.
        /// </summary>
        internal string PropertyData
        {
            get
            {
                return this._propertyData;
            }
        }

        public bool Any()
        {
            return Enumerable.Any(this);
        }

        public IEnumerator<RelatedLink> GetEnumerator()
        {
            return _relatedLinks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
