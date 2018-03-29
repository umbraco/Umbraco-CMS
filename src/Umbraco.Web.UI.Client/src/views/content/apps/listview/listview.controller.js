(function () {
    "use strict";

    function ContentAppListViewController($scope) {

        var vm = this;

        vm.listViewGroup = {};

        function onInit() {
            angular.forEach($scope.model.tabs, function(group){
                if(group.alias === "umbContainerView") {
                    vm.listViewGroup = group;
                }
            });
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.Apps.ListViewController", ContentAppListViewController);
})();
