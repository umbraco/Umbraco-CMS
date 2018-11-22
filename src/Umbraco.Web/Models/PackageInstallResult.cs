using System;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// Model that is returned when a package is totally finished installing
    /// </summary>
    public class PackageInstallResult : PackageInstallModel
    {
        [DataMember(Name = "postInstallationPath")]
        public string PostInstallationPath { get; set; }
    }
}