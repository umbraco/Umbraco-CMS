angular.module("umbraco").controller("Umbraco.Dialogs.LoginController", function ($scope, userService) {
    
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
        
        userService.authenticate(login, password)
            .then(function (data) {
                $scope.submit(data);
            }, function (reason) {
                alert(reason);
            });
    };
});