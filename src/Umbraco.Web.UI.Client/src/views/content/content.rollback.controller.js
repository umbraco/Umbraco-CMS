(function () {
    "use strict";

    function ContentRollbackController($scope, $timeout) {

        var vm = this;

        vm.rollback = rollback;
        vm.loadVersion = loadVersion;
        vm.closeDialog = closeDialog;

        function onInit() {

            vm.loading = true;
            vm.variantVersions = [];

            // fake load versions
            var currentLanguage = $scope.currentNode.metaData.culture;
            $timeout(function(){
                vm.versions = {
                    "currentVersion": {
                        "id": 1,
                        "name": "Variant name (Created: 22/08/2018 13.32)"
                    },
                    "previousVersions": [
                        {
                            "id": 1,
                            "name": "Variant name (Created: 22/08/2018 13.32)"
                        },
                        {
                            "id": 2,
                            "name": "Variant name (Created: 21/08/2018 19.25)"
                        },
                        {
                            "id": 3,
                            "name": "Variant name (Created: 15/08/2018 22.11)"
                        }
                    ]
                };
                vm.loading = false;
            }, 200);
        }

        function rollback() {
            console.log("rollback");
        }

        /**
         * This will load in a new version
         */
        function loadVersion(id) {
            console.log("load version", id);
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