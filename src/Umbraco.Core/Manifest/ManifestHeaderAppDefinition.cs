using System;
using System.Runtime.Serialization;
using Umbraco.Core.IO;

namespace Umbraco.Core.Manifest
{
    // headerApps: [
    //   {
    //     name: 'App Name',        // required
    //     alias: 'appAlias',       // required
    //     weight: 0,               // optional, default is 0, use values between -99 and +99
    //     icon: 'icon.app',        // required
    //     view: 'path/view.htm',   // required if you are not using the action value
    //     action: ''               // required if you have not created your own view
    //     show: [                  // optional, default is always show
    //       '+role/admin'          // show for admin users. Role based permissions will override others.
    //     ]
    //   },
    //   ...
    // ]

    [DataContract(Name = "headerappdef", Namespace = "")]
    public class ManifestHeaderAppDefinition 
    {
        private string _view;

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        [DataMember(Name = "weight")]
        public int Weight { get; set; }

        [DataMember(Name = "icon")]
        public string Icon { get; set; }

        [DataMember(Name = "view")]
        public string View
        {
            get => _view;
            set => _view = IOHelper.ResolveVirtualUrl(value);
        }

        [DataMember(Name = "action")]
        public string Action { get; set; }

        [DataMember(Name = "show")]
        public string[] Show { get; set; } = Array.Empty<string>();
    }
}
