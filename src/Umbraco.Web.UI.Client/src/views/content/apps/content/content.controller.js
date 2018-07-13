(function () {
    "use strict";

    function ContentAppContentController($scope) {

        var vm = this;

        function onInit() {

            //select the first one in the list
            //TODO: We need to track the active one
            vm.content = $scope.model.variants[0];
            vm.content.active = true;

            angular.forEach(vm.content.tabs, function(group){
                group.open = true;
            });
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.Apps.ContentController", ContentAppContentController);
})();
