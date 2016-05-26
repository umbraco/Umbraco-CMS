(function () {
    "use strict";

    function PackagesEditController($scope) {

        var vm = this;

        vm.page = {};
        vm.page.name = "Packages";

        vm.categories = [
            {
                "icon": "icon-male-and-female",
                "name": "Collaboration"
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
                "name": "Test App",
                "description": "lorem flaxis slk asjasd ks",
                "karma": "1",
                "downloads": "2",
                "icon": "flax"
            },
            {
                "name": "Tessti flaxi",
                "description": "loremlaksjd lkajs dasjasd ks",
                "karma": "10",
                "downloads": "22",
                "icon": "flaxo"
            },
            {
                "name": "Walla",
                "description": "lorem flaxis slk asjasd ks",
                "karma": "1",
                "downloads": "2",
                "icon": "flax"
            },
            {
                "name": "Walla",
                "description": "lorem flaxis slk asjasd ks",
                "karma": "1",
                "downloads": "2",
                "icon": "flax"
            },
            {
                "name": "Test App",
                "description": "lorem flaxis slk asjasd ks",
                "karma": "1",
                "downloads": "2",
                "icon": "flax"
            },
            {
                "name": "Tessti flaxi",
                "description": "loremlaksjd lkajs dasjasd ks",
                "karma": "10",
                "downloads": "22",
                "icon": "flaxo"
            },
            {
                "name": "Walla",
                "description": "lorem flaxis slk asjasd ks",
                "karma": "1",
                "downloads": "2",
                "icon": "flax"
            },
            {
                "name": "Walla",
                "description": "lorem flaxis slk asjasd ks",
                "karma": "1",
                "downloads": "2",
                "icon": "flax"
            }
        ];


    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.EditController", PackagesEditController);

})();
