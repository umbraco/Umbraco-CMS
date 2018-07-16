(function () {
    "use strict";

    function MediaAppContentController($scope) {

        var vm = this;

        function onInit() {
            angular.forEach($scope.content.tabs, function(group){
                group.open = true;
            });
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Media.Apps.ContentController", MediaAppContentController);
})();
