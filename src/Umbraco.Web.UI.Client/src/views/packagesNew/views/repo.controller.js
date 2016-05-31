(function () {
    "use strict";

    function PackagesRepoController($scope, $route, $location) {

        var vm = this;

        vm.selectCategory = selectCategory;
        vm.showPackageDetails = showPackageDetails;

        vm.categories = [
            {
                "id": 1,
                "icon": "icon-male-and-female",
                "name": "All",
                "active": true
            },
            {
                "icon": "icon-male-and-female",
                "name": "Collaboration"
            },
            {
                "id": 2,
                "icon": "icon-molecular-network",
                "name": "Backoffice extensions"
            },
            {
                "id": 3,
                "icon": "icon-brackets",
                "name": "Developer tools"
            },
            {
                "id": 4,
                "icon": "icon-wand",
                "name": "Starter kits"
            },
            {
                "id": 5,
                "icon": "icon-medal",
                "name": "Umbraco Pro"
            },
            {
                "id": 6,
                "icon": "icon-wrench",
                "name": "Website utilities"
            }
        ];

        vm.packages = [
            {
                "id": 1,
                "name": "uSightly",
                "description": "An HTML5 audio player based on jPlayer",
                "karma": "1",
                "downloads": "1672",
                "icon":"https://our.umbraco.org/media/wiki/150283/635768313097111400_usightlylogopng.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 2,
                "name": "Kill IE6",
                "description": "A simple port of the IE6 warning script (http://code.google.com/p/ie6-upgrade-warning/) to use in your Umbraco websites.",
                "karma": "11",
                "downloads": "688",
                "icon":"https://our.umbraco.org/media/wiki/9138/634697622367666000_offroadcode-100x100.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 3,
                "name": "Examine Media Indexer",
                "description": "CogUmbracoExamineMediaIndexer",
                "karma": "3",
                "downloads": "1329",
                "icon":"https://our.umbraco.org/media/wiki/50703/634782902373558000_cogworks.jpg?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 4,
                "name": "SVG Icon Picker",
                "description": "A picker, for picking icons from an SVG spritesheet.",
                "karma": "5",
                "downloads": "8",
                "icon":"https://our.umbraco.org/media/wiki/154472/635997115126742822_logopng.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 5,
                "name": "Pipeline CRM",
                "description": "Pipeline is a social CRM that lives in Umbraco back-office. It tracks opportunities and helps teams collaborate with timelines and tasks. It stores information about your customers and your interactions with them. It integrates with your website, capturing opportunities from forms and powering personal portals.",
                "karma": "3",
                "downloads": "105",
                "icon":"https://our.umbraco.org/media/wiki/152476/635917291068518788_pipeline-crm-logopng.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 6,
                "name": "uSightly",
                "description": "An HTML5 audio player based on jPlayer",
                "karma": "1",
                "downloads": "1672",
                "icon":"https://our.umbraco.org/media/wiki/150283/635768313097111400_usightlylogopng.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 7,
                "name": "Kill IE6",
                "description": "A simple port of the IE6 warning script (http://code.google.com/p/ie6-upgrade-warning/) to use in your Umbraco websites.",
                "karma": "11",
                "downloads": "688",
                "icon":"https://our.umbraco.org/media/wiki/9138/634697622367666000_offroadcode-100x100.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 8,
                "name": "Examine Media Indexer",
                "description": "CogUmbracoExamineMediaIndexer",
                "karma": "3",
                "downloads": "1329",
                "icon":"https://our.umbraco.org/media/wiki/50703/634782902373558000_cogworks.jpg?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 9,
                "name": "SVG Icon Picker",
                "description": "A picker, for picking icons from an SVG spritesheet.",
                "karma": "5",
                "downloads": "8",
                "icon":"https://our.umbraco.org/media/wiki/154472/635997115126742822_logopng.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 10,
                "name": "Pipeline CRM",
                "description": "Pipeline is a social CRM that lives in Umbraco back-office. It tracks opportunities and helps teams collaborate with timelines and tasks. It stores information about your customers and your interactions with them. It integrates with your website, capturing opportunities from forms and powering personal portals.",
                "karma": "3",
                "downloads": "105",
                "icon":"https://our.umbraco.org/media/wiki/152476/635917291068518788_pipeline-crm-logopng.png?bgcolor=fff&height=154&width=281&format=png"
            }
        ];

        function selectCategory(selectedCategory, categories) {
            for (var i = 0; i < categories.length; i++) {
                var category = categories[i];
                category.active = false;
            }
            selectedCategory.active = true;
        }

        function showPackageDetails(selectedPackage) {
            var section = $route.current.params.section;
            var tree = $route.current.params.tree;
            var path = "/" + section + "/" + tree + "/details/" + selectedPackage.id;
            $location.path(path);
        }


    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.RepoController", PackagesRepoController);

})();
