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
    var d = new Date();
    var weekday = new Array("Super Sunday", "Manic Monday", "Tremendous Tuesday", "Wonderfull Wednesday", "Thunder Thursday", "Friendly Friday", "Shiny Saturday");
    $scope.today = weekday[d.getDay()];

    $scope.loginClick = function (login, password) {
        
        userService.authenticate(login, password)
            .then(function (data) {
                $scope.submit(data);
            }, function (reason) {
                alert(reason);
            });
    };
});