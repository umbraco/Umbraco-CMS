(function () {
    "use strict";

    function ContentPermissionsController($scope, $timeout) {

        var vm = this;

        vm.loading = false;
        vm.permissions = [];
        vm.users = [];
        vm.saveButtonState = "init";

        vm.save = save;

        function activate() {

            vm.loading = true;

            // fake loading
            $timeout(function () {

                vm.loading = false;

                vm.permissions = [
                    {
                        "name": "Culture and Hostnames",
                        "users": []
                    },
                    {
                        "name": "Audit Trail",
                        "users": []
                    },
                    {
                        "name": "Browse Node",
                        "users": [1, 2, 3]
                    },
                    {
                        "name": "Change Document Type",
                        "users": []
                    },
                    {
                        "name": "Copy",
                        "users": [1, 4]
                    },
                    {
                        "name": "Delete",
                        "users": []
                    },
                    {
                        "name": "Move",
                        "users": [3, 4]
                    },
                    {
                        "name": "Create",
                        "users": [1]
                    }
                ];

                vm.users = [
                    {
                        "name": "User 1",
                        "id": 1
                    },
                    {
                        "name": "User 2",
                        "id": 2
                    },
                    {
                        "name": "User 3",
                        "id": 3
                    },
                                        {
                        "name": "User 3",
                        "id": 4
                    },
                ];

            }, 1000);
        }

        function save() {

            vm.saveButtonState = "busy";

            // fake loading
            $timeout(function () {
                vm.saveButtonState = "success";
            }, 1000);

        }

        activate();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.PermissionsController", ContentPermissionsController);
})();