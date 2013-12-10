
/**
 * @ngdoc controller
 * @name Umbraco.MainController
 * @function
 * 
 * @description
 * The controller for the AuthorizeUpgrade login page
 * 
 */
function AuthorizeUpgradeController($scope, $window) {
    
    //Add this method to the scope - this method will be called by the login dialog controller when the login is successful
    // then we'll handle the redirect.
    $scope.submit = function (event) {

        var qry = $window.location.search.trimStart("?").split("&");
        var redir = _.find(qry, function(item) {
            return item.startsWith("redir=");
        });
        if (redir) {
            $window.location = decodeURIComponent(redir.split("=")[1]);
        }
        else {
            $window.location = "/";
        }
        
    };

}

angular.module('umbraco').controller("Umbraco.AuthorizeUpgradeController", AuthorizeUpgradeController);