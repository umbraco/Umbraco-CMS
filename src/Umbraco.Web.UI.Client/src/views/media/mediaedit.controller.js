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

    $scope.files = [];
    $scope.addFiles = function (propertyId, files) {
        //this will clear the files for the current property and then add the new ones for the current property
        $scope.files = _.reject($scope.files, function (item) {
            return item.id == propertyId;
        });
        for (var i = 0; i < files.length; i++) {
            //save the file object to the scope's files collection
            $scope.files.push({ id: propertyId, file: files[i] });
        }
    };

    $scope.save = function (cnt) {
        cnt.updateDate = new Date();
        mediaResource.saveMedia(cnt, $routeParams.create, $scope.files);
        notificationsService.success("Saved", "Media has been saved");
    };
}

angular.module("umbraco")
    .controller("Umbraco.Editors.MediaEditController", mediaEditController);