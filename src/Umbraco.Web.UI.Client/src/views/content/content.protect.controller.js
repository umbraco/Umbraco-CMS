(function () {
    "use strict";

    function ContentProtectController($scope, $routeParams, contentResource, memberGroupResource, navigationService) {

        var vm = this;
        var id = $scope.currentNode.id;

        vm.loading = false;
        vm.saveButtonState = "init";

        vm.next = next;
        vm.save = save;
        vm.close = close;

        vm.type = null;

        function onInit() {
            vm.loading = true;
            // Get all member groups
            memberGroupResource.getGroups().then(function (groups) {                
                vm.groups = groups;
                vm.step = "type";
                vm.loading = false;
            });
        }

        function next() {
            vm.step = vm.type;
        }

        function save() {
            vm.saveButtonState = "busy";
            console.log("TODO: SAVE!")
        }

        function close() {
            navigationService.hideDialog();
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.ProtectController", ContentProtectController);
})();
