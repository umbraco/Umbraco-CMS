(function () {
    "use strict";

    function ContentRepublishController($scope, $timeout) {

        var vm = this;

        vm.republishButtonState = "init";

        vm.republish = republish;

        function republish() {

            vm.republishButtonState = "busy";

            // fake loading
            $timeout(function () {
                vm.republishButtonState = "success";
            }, 1000);

        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.RepublishController", ContentRepublishController);
})();
