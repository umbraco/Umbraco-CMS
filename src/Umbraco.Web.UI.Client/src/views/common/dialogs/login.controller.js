angular.module("umbraco").controller("Umbraco.Dialogs.LoginController", function ($scope, userService, legacyJsLoader) {
    
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

                //We need to load in the legacy tree js.
                legacyJsLoader.loadLegacyTreeJs($scope).then(
                    function(result) {
                        //TODO: We could wait for this to load before running the UI ?
                    });
                
                $scope.submit(data);

            }, function (reason) {
                alert(reason);
            });
    };
});