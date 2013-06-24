/**
 * @ngdoc controller
 * @name LegacyController
 * @function
 * 
 * @description
 * A controller to control the legacy iframe injection
 * 
 * @param myParam {object} Enter param description here
*/
function LegacyController($scope, $routeParams, $element) {
    //set the legacy path
    $scope.legacyPath = decodeURIComponent($routeParams.url);
    
    //$scope.$on('$routeChangeSuccess', function () {
    //    var asdf = $element;
    //});
}
angular.module("umbraco").controller('LegacyController', LegacyController);