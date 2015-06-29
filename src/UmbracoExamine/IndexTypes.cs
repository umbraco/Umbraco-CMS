﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Store;

namespace UmbracoExamine
{
    /// <summary>
    /// The index types stored in the Lucene Index
    /// </summary>
    public static class IndexTypes
    {

        /// <summary>
        /// The content index type        
        /// </summary>
        /// <remarks>
        /// Is lower case because the Standard Analyzer requires lower case
        /// </remarks>
        public const string Content = "content";

        /// <summary>
        /// The media index type        
        /// </summary>
        /// <remarks>
        /// Is lower case because the Standard Analyzer requires lower case
        /// </remarks>
        public const string Media = "media";

        /// <summary>
        /// The member index type        
        /// </summary>
        /// <remarks>
        /// Is lower case because the Standard Analyzer requires lower case
        /// </remarks>
        public const string Member = "member";
    }
}
