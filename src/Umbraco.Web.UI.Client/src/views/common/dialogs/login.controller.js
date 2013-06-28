angular.module("umbraco").controller("Umbraco.Dialogs.LoginController", function ($scope, userService) {
    alert("login");
   
    /**
     * @ngdoc function
     * @name signin
     * @methodOf MainController
     * @function
     *
     * @description
     * signs the user in
     */
    $scope.loginClick = function (login, password) {
        alert("wat");
        
        userService.authenticate(login, password)
            .then(function (data) {
                $scope.authenticated = data.authenticated;
                $scope.user = data.user;
            }, function (reason) {
                alert(reason);
            });
    };
});