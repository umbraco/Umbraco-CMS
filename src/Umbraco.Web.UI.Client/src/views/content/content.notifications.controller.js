(function () {
    "use strict";

    function ContentNotificationsController($scope, $timeout) {

        var vm = this;

        vm.loading = false;
        vm.actions = [];
        vm.saveButtonState = "init";

        vm.save = save;

        function activate() {

            vm.loading = true;

            // fake loading
            $timeout(function () {

                vm.loading = false;

                vm.actions = [
                    {
                        "name": "Culture and Hostnames",
                        "selected": true
                    },
                    {
                        "name": "Audit Trail",
                        "selected": false
                    },
                    {
                        "name": "Browse Node",
                        "selected": true
                    },
                    {
                        "name": "Change Document Type",
                        "selected": false
                    },
                    {
                        "name": "Copy",
                        "selected": false
                    },
                    {
                        "name": "Delete",
                        "selected": true
                    },
                    {
                        "name": "Move",
                        "selected": false
                    },
                    {
                        "name": "Create",
                        "selected": false
                    }
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

    angular.module("umbraco").controller("Umbraco.Editors.Content.NotificationsController", ContentNotificationsController);
})();