using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Represents a content app definition, parsed from a manifest.
    /// </summary>
    [DataContract(Name = "appdef", Namespace = "")]
    public class ManifestContentAppDefinition : IContentAppDefinition
    {
        private static readonly Regex ShowRegex = new Regex("^([+-])?([a-z]+)/([a-z0-9_]+|\\*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private string _view;
        private ContentApp _app;

        /// <summary>
        /// Gets or sets the name of the content app.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique alias of the content app.
        /// </summary>
        /// <remarks>
        /// <para>Must be a valid javascript identifier, ie no spaces etc.</para>
        /// </remarks>
        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the icon of the content app.
        /// </summary>
        /// <remarks>
        /// <para>Must be a valid helveticons class name (see http://hlvticons.ch/).</para>
        /// </remarks>
        [DataMember(Name = "icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the view for rendering the content app.
        /// </summary>
        [DataMember(Name = "view")]
        public string View
        {
            get => _view;
            set => _view = IOHelper.ResolveVirtualUrl(value);
        }

        /// <summary>
        /// Gets or sets the list of 'show' conditions for the content app.
        /// </summary>
        [DataMember(Name = "show")]
        public string[] Show { get; set; } = Array.Empty<string>();

        /// <inheritdoc />
        public ContentApp GetContentAppFor(object o)
        {
            string entityType, contentType;

            switch (o)
            {
                case IContent content:
                    entityType = "content";
                    contentType = content.ContentType.Alias;
                    break;

                case IMedia media:
                    entityType = "media";
                    contentType = media.ContentType.Alias;
                    break;

                default:
                    return null;
            }

            // if no 'show' is specified, then always display the content app
            if (Show.Length > 0)
            {
                var ok = false;

                // else iterate over each entry
                foreach (var show in Show)
                {
                    var match = ShowRegex.Match(show);
                    if (!match.Success)
                        throw new FormatException($"Illegal 'show' entry \"{show}\" in manifest.");

                    var e = match.Groups[2].Value;
                    var c = match.Groups[3].Value;

                    // if the entry does not apply, skip it
                    // support wildcards for entity & content types
                    if ((e != "*" && !e.InvariantEquals(entityType)) || (c != "*" && !c.InvariantEquals(contentType)))
                        continue;

                    // if the entry applies,
                    // if it's an exclude entry, exit, do not display the content app
                    if (match.Groups[1].Value == "-")
                        return null;

                    // else break - ok to display
                    ok = true;
                    break;
                }

                // when 'show' is specified, default is to *not* show the content app
                if (!ok)
                    return null;
            }

            // content app can be displayed
            return _app ?? (_app = new ContentApp
            {
                Alias = Alias,
                Name = Name,
                Icon = Icon,
                View = View
            });
        }
    }
}
