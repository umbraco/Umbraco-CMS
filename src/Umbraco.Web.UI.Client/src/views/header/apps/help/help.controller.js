(function () {
    "use strict";

    function HeaderAppHelpController($scope, appState) {
        // toggle the help dialog by raising the global app state to toggle the help drawer
        $scope.helpClick = function () {
            var showDrawer = appState.getDrawerState("showDrawer");
            var drawer = { view: "help", show: !showDrawer };
            appState.setDrawerState("view", drawer.view);
            appState.setDrawerState("showDrawer", drawer.show);
        };
    }

    angular.module("umbraco").controller("Umbraco.Editors.Header.Apps.HelpController", HeaderAppHelpController);
})();
