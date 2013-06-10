function mediaEditController($scope, $routeParams, contentResource, notificationsService) {
    if ($routeParams.create)
        $scope.content = contentResource.getContentScaffold($routeParams.id, $routeParams.doctype);
    else
        $scope.content = contentResource.getContent($routeParams.id);


    $scope.saveAndPublish = function (cnt) {
        cnt.publishDate = new Date();
        contentResource.publishContent(cnt);
        notificationsService.success("Published", "Content has been saved and published");
    };

    $scope.save = function (cnt) {
        cnt.updateDate = new Date();
        contentResource.saveContent(cnt);
        notificationsService.success("Saved", "Content has been saved");
    };
}

angular.module("umbraco")
    .controller("Umbraco.Editors.MediaEditController", mediaEditController);