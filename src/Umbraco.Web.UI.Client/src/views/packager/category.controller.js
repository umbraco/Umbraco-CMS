(function () {
    "use strict";

    function PackagesCategoryController($scope, $routeParams) {

        var vm = this;

        vm.page = {};
        vm.page.name = "Category";

        vm.selectCategory = selectCategory;

        vm.categories = [
            {
                "icon": "icon-male-and-female",
                "name": "All",
                "active": false
            },
            {
                "icon": "icon-male-and-female",
                "name": "Collaboration",
                "active": true
            },
            {
                "icon": "icon-molecular-network",
                "name": "Backoffice extensions"
            },
            {
                "icon": "icon-brackets",
                "name": "Developer tools"
            },
            {
                "icon": "icon-wand",
                "name": "Starter kits"
            },
            {
                "icon": "icon-medal",
                "name": "Umbraco Pro"
            },
            {
                "icon": "icon-wrench",
                "name": "Website utilities"
            }
        ];

        vm.packages = [
            {
                "name": "uSightly",
                "description": "An HTML5 audio player based on jPlayer",
                "karma": "1",
                "downloads": "1672",
                "icon":"https://our.umbraco.org/media/wiki/150283/635768313097111400_usightlylogopng.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "name": "Kill IE6",
                "description": "A simple port of the IE6 warning script (http://code.google.com/p/ie6-upgrade-warning/) to use in your Umbraco websites.",
                "karma": "11",
                "downloads": "688",
                "icon":"https://our.umbraco.org/media/wiki/9138/634697622367666000_offroadcode-100x100.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "name": "Examine Media Indexer",
                "description": "CogUmbracoExamineMediaIndexer",
                "karma": "3",
                "downloads": "1329",
                "icon":"https://our.umbraco.org/media/wiki/50703/634782902373558000_cogworks.jpg?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "name": "SVG Icon Picker",
                "description": "A picker, for picking icons from an SVG spritesheet.",
                "karma": "5",
                "downloads": "8",
                "icon":"https://our.umbraco.org/media/wiki/154472/635997115126742822_logopng.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "name": "Pipeline CRM",
                "description": "Pipeline is a social CRM that lives in Umbraco back-office. It tracks opportunities and helps teams collaborate with timelines and tasks. It stores information about your customers and your interactions with them. It integrates with your website, capturing opportunities from forms and powering personal portals.",
                "karma": "3",
                "downloads": "105",
                "icon":"https://our.umbraco.org/media/wiki/152476/635917291068518788_pipeline-crm-logopng.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "name": "CodeMirror",
                "description": "CodeMirror Editor for Umbraco",
                "karma": "1",
                "downloads": "70",
                "icon":"https://our.umbraco.org/media/wiki/151028/635810233171153461_logopng.png?bgcolor=fff&height=154&width=281&format=png"
            }
        ];

        function selectCategory(category) {

        }


    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.CategoryController", PackagesCategoryController);

})();
