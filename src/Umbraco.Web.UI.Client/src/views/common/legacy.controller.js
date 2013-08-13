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
    //set the legacy path
    $scope.legacyPath = decodeURIComponent($routeParams.url);
    
    //$scope.$on('$routeChangeSuccess', function () {
    //    var asdf = $element;
    //});
}
angular.module("umbraco").controller('Umbraco.LegacyController', LegacyController);