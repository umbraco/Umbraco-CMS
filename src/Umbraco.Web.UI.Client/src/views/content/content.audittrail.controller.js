(function () {
    "use strict";

    function ContentAuditTrailController($scope, $timeout) {

        var vm = this;

        vm.loading = false;
        vm.auditTrail = [];


        function activate() {

            vm.loading = true;

            // fake loading
            $timeout(function () {

                vm.loading = false;

                vm.auditTrail = [
                    {
                        "action": "Save",
                        "user": "User Name",
                        "date": "Date",
                        "comment": "Save content performed by user"
                    },
                    {
                        "action": "Save",
                        "user": "User Name",
                        "date": "Date",
                        "comment": "Save content performed by user"
                    },
                    {
                        "action": "Publish",
                        "user": "User Name",
                        "date": "Date",
                        "comment": "Save and publish performed by user"
                    }
                ];

            }, 1000);
        }

        activate();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.AuditTrailController", ContentAuditTrailController);
})();
