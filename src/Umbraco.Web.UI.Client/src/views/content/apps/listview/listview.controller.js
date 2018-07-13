(function () {
    "use strict";

    function ContentAppListViewController($scope) {

        var vm = this;

        vm.listViewGroup = {};

        //TODO: We need to fix this, there is no umbContainerView anymore since this worked as a hack by copying
        // across a tab/property editor to a content App, instead we are going to need to hack the list view a different
        // way since currently it still requires us to use umb-property-editor so we'll either need to construct the model
        // here ourselves or somehow allow the server to pass in a model for a content app.

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
