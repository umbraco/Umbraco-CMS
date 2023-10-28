
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
    
    $scope.loginAndRedirect = function (event) {

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