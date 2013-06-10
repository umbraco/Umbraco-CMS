function mediaEditController($scope, $routeParams, mediaResource, notificationsService) {

    if ($routeParams.create) {

        mediaResource.getContentScaffold($routeParams.id, $routeParams.doctype)
            .then(function (data) {
                $scope.content = data;
            });
    }
    else {
        mediaResource.getById($routeParams.id)
            .then(function (data) {
                $scope.content = data;
            });
    }

    $scope.save = function (cnt) {
        cnt.updateDate = new Date();
        contentResource.saveContent(cnt);
        notificationsService.success("Saved", "Media has been saved");
    };
}

angular.module("umbraco")
    .controller("Umbraco.Editors.MediaEditController", mediaEditController);