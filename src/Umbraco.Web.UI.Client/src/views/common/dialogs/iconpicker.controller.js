//used for the icon picker dialog
angular.module("umbraco")
    .controller("Umbraco.Dialogs.IconPickerController",
        function ($scope, iconHelper) {
            $scope.icons = iconHelper.getIcons("");
});