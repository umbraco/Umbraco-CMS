/**
 * @ngdoc controller
 * @name Umbraco.Editors.Templates.InsertMacroController
 * @function
 * 
 * @description
 * The controller for the custom insert macro dialog. Until we upgrade the template editor to be angular this 
 * is actually loaded into an iframe with full html.
 */
function InsertMacroController($scope, userService) {
 
    //fetch the authorized status         
    userService.isAuthenticated()
        .then(function (data) {
            
            $scope.authenticated = data.authenticated;
            $scope.user = data.user;

        }, function (reason) {
            alert("An error occurred checking authentication.");
            //TODO: We'd need to proxy this call to the main window
            //notificationsService.error("An error occurred checking authentication.");
            $scope.authenticated = false;
            $scope.user = null;
        });

}

angular.module("umbraco").controller("Umbraco.Editors.Templates.InsertMacroController", InsertMacroController);
