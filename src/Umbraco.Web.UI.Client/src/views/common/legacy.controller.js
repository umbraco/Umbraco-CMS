/**
 * @ngdoc controller
 * @name Umbraco.LegacyController
 * @function
 * 
 * @description
 * A controller to control the legacy iframe injection
 * 
*/
function LegacyController($scope, $routeParams, $element) {

    var url = $routeParams.url;
    var toClean = "*?(){}[];:%<>/\\|&'\"";
    for (var i = 0; i < toClean.length; i++) {
        url = url.replace(toClean[i], "");
    }
    $scope.legacyPath = decodeURIComponent(url);
}

angular.module("umbraco").controller('Umbraco.LegacyController', LegacyController);