﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Umbraco.Core.Collections;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// The culture names of a content's variants
    /// </summary>
    public class ContentCultureInfosCollection : ObservableDictionary<string, ContentCultureInfos>, IDeepCloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentCultureInfosCollection"/> class.
        /// </summary>
        public ContentCultureInfosCollection()
            : base(x => x.Culture, StringComparer.InvariantCultureIgnoreCase)
        { }
        
        /// <summary>
        /// Adds or updates a <see cref="ContentCultureInfos"/> instance.
        /// </summary>
        public void AddOrUpdate(string culture, string name, DateTime date)
        {
            if (culture.IsNullOrWhiteSpace()) throw new ArgumentNullOrEmptyException(nameof(culture));
            culture = culture.ToLowerInvariant();

            if (TryGetValue(culture, out var item))
            {
                item.Name = name;
                item.Date = date;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, item));
            }
            else
            {
                Add(new ContentCultureInfos(culture)
                {
                    Name = name,
                    Date = date
                });
            }
        }

        /// <inheritdoc />
        public object DeepClone()
        {
            var clone = new ContentCultureInfosCollection();

            foreach (var item in this)
            {
                var itemClone = (ContentCultureInfos) item.DeepClone();
                itemClone.ResetDirtyProperties(false);
                clone.Add(itemClone);
            }

            return clone;
        }
    }
}
