function mediaCreateController ($scope, $routeParams,mediaTypeResource) {
    $scope.allowedTypes = mediaTypeResource.getAllowedTypes($scope.currentNode.id);
}

angular.module('umbraco')
    .controller("Umbraco.Editors.Media.CreateController", mediaCreateController);