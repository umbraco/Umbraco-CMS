(function () {
    "use strict";

    function HeaderAppSearchController($scope, focusService, appState) {
        $scope.rememberFocus = focusService.rememberFocus;

        $scope.searchClick = function () {
            var showSearch = appState.getSearchState("show");
            appState.setSearchState("show", !showSearch);
        };
    }

    angular.module("umbraco").controller("Umbraco.Editors.Header.Apps.SearchController", HeaderAppSearchController);
})();
