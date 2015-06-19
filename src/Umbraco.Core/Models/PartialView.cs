using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Umbraco.Core.IO;

namespace Umbraco.Core.Models
{
  
    /// <summary>
    /// Represents a Partial View file
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class PartialView : File, IPartialView
    {

        public PartialView(string path)
            : base(path)
        {
            base.Path = path;
        }

    }
}