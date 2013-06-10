function mediaCreateController ($scope, $routeParams,contentTypeResource) {
    $scope.allowedTypes  = contentTypeResource.getAllowedTypes($scope.currentNode.id);
}

angular.module('umbraco')
    .controller("Umbraco.Editors.MediaCreateController", mediaCreateController);