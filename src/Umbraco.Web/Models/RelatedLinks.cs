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

        private readonly IEnumerable<RelatedLink> _relatedLinks;

        public RelatedLinks(IEnumerable<RelatedLink> relatedLinks, string propertyData)
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
