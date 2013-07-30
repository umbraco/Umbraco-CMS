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

    $scope.errorMsg = "";
    
    $scope.loginSubmit = function (login, password) {
        
        if ($scope.loginForm.$invalid) {
            return;
        }

        userService.authenticate(login, password)
            .then(function (data) {
                //We need to load in the legacy tree js.
                legacyJsLoader.loadLegacyTreeJs($scope).then(
                    function(result) {
                        $scope.submit(true);
                    });
            }, function (reason) {
                $scope.errorMsg = reason.errorMsg;
                
                //set the form inputs to invalid
                $scope.loginForm.username.$setValidity("auth", false);
                $scope.loginForm.password.$setValidity("auth", false);
            });
        
        //setup a watch for both of the model values changing, if they change
        // while the form is invalid, then revalidate them so that the form can 
        // be submitted again.
        $scope.loginForm.username.$viewChangeListeners.push(function () {
            if ($scope.loginForm.username.$invalid) {
                $scope.loginForm.username.$setValidity('auth', true);
            }
        });
        $scope.loginForm.password.$viewChangeListeners.push(function () {
            if ($scope.loginForm.password.$invalid) {
                $scope.loginForm.password.$setValidity('auth', true);
            }
        });
    };
});