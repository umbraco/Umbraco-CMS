angular.module("umbraco")
    .controller("Umbraco.Dialogs.UserController", function ($scope, $location) {
        $scope.logout = function () {
        userService.logout();
        $location = "/";
    };
});