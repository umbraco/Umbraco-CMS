(function () {
    "use strict";

    function ContentRollbackController($scope, assetsService) {

        var vm = this;

        vm.rollback = rollback;
        vm.loadVersion = loadVersion;
        vm.closeDialog = closeDialog;

        function onInit() {

            vm.loading = true;
            vm.variantVersions = [];
            vm.diff = null;

            // Load in diff library
            assetsService.loadJs('lib/jsdiff/diff.min.js', $scope).then(function () {

                var currentLanguage = $scope.currentNode.metaData.culture;

                vm.currentVersion = {
                    "id": 1,
                    "createDate": "22/08/2018 13.32",
                    "name": "Forside",
                    "properties": [
                        {
                            "alias": "headline",
                            "label": "Headline",
                            "value": "Velkommen"
                        },
                        {
                            "alias": "text",
                            "label": "Text",
                            "value": "This is my danish Content"
                        }
                    ]
                };

                vm.previousVersions = [
                    {
                        "id": 2,
                        "name": "Forside",
                        "createDate": "21/08/2018 19.25"
                    }
                ];

                vm.loading = false;

            });

        }

        function rollback() {
            console.log("rollback");
        }

        /**
         * This will load in a new version
         */
        function loadVersion(id) {

            // fake load version
            var currentLanguage = $scope.currentNode.metaData.culture;

            vm.diff = {};
            vm.diff.properties = [];

            var oldVersion = {
                "id": 2,
                "name": "Foride",
                "properties": [
                    {
                        "alias": "headline",
                        "label": "Headline",
                        "value": ""
                    },
                    {
                        "alias": "text",
                        "label": "Text",
                        "value": "This is my danish Content Test"
                    }
                ]
            };

            // find diff in name
            vm.diff.name = JsDiff.diffWords(vm.currentVersion.name, oldVersion.name);

            // find diff in properties
            angular.forEach(vm.currentVersion.properties, 
                function(newProperty, index){
                    var oldProperty = oldVersion.properties[index];
                    var diffProperty = {
                        "alias": newProperty.alias,
                        "label": newProperty.label,
                        "diff": JsDiff.diffWords(newProperty.value, oldProperty.value)
                    };
                    vm.diff.properties.push(diffProperty);
                });

        }

        /**
         * This will close the dialog
         */
        function closeDialog() {
            $scope.nav.hideDialog();
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.RollbackController", ContentRollbackController);

})();