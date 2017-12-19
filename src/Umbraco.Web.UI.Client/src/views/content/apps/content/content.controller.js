(function () {
    "use strict";

    function ContentAppContentController($scope) {

        var vm = this;

        function onInit() {
            angular.forEach($scope.model.tabs, function(group){
                group.open = true;
            });
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.Apps.ContentController", ContentAppContentController);
})();
