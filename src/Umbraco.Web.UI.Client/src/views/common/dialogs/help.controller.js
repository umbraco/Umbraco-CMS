angular.module("umbraco")
    .controller("Umbraco.Dialogs.HelpController", function ($scope, $location, $routeParams) {
        $scope.section = $routeParams.section;
        if(!$scope.section){
            $scope.section ="content";
        }
    });