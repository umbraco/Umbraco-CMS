(function () {
    "use strict";

    function MediaAppListViewController($scope) {

        var vm = this;

        vm.listViewGroup = {};

        function onInit() {
            angular.forEach($scope.model.tabs, function(group){
                if(group.alias === "Contents") {
                    vm.listViewGroup = group;
                }
            });
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Media.Apps.ListViewController", MediaAppListViewController);
})();