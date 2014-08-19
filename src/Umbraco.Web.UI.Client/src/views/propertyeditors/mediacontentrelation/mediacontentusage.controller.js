angular.module("umbraco").controller("Chalmers.MediaContentUsageController", function ($scope, editorState, mediaContentUsageResource, notificationsService) {

    // Check the pre-values for showing the path
    $scope.showPath = $scope.model.config.showPath && $scope.model.config.showPath !== '0' ? true : false;

    // Indicate that we are loading
    $scope.loading = true;

    // Get data about Content usage from our api resource
    mediaContentUsageResource.getMediaContentUsage(editorState.current.id)
        .then(function (response) {
            $scope.media = response.data;
            $scope.loading = false;
        });
});
