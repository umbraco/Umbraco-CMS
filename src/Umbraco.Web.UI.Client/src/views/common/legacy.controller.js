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

    var url = decodeURIComponent($routeParams.url.toLowerCase().trimStart("javascript:"));
    var toClean = "*(){}[];:<>\\|'\"";
    for (var i = 0; i < toClean.length; i++) {
        var reg = new RegExp("\\" + toClean[i], "g");
        url = url.replace(reg, "");
    }
    $scope.legacyPath = url;
}

angular.module("umbraco").controller('Umbraco.LegacyController', LegacyController);