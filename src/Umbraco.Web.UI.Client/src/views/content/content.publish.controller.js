(function () {
    "use strict";

    function ContentPublishController($scope, $timeout) {

        var vm = this;

        vm.publishButtonState = "init";
        vm.publishAll = false;
        vm.includeUnpublished = false;

        vm.publish = publish;

        function publish() {

            vm.publishButtonState = "busy";

            console.log(vm.publishAll);
            console.log(vm.includeUnpublished);

            // fake loading
            $timeout(function () {
                vm.publishButtonState = "success";
            }, 1000);

        }


    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.PublishController", ContentPublishController);
})();