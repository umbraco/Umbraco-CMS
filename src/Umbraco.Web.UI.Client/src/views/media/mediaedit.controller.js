function mediaEditController($scope, $routeParams, mediaResource, notificationsService) {

    if ($routeParams.create) {

        mediaResource.getScaffold($routeParams.id, $routeParams.doctype)
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
    
    //TODO: Clean this up and share this code with the content editor
    $scope.saveAndPublish = function (cnt) {        
        mediaResource.saveMedia(cnt, $routeParams.create, $scope.files)
            .then(function (data) {
                $scope.content = data;
                notificationsService.success("Published", "Media has been saved and published");
            });
    };

    //TODO: Clean this up and share this code with the content editor
    $scope.save = function (cnt) {        
        mediaResource.saveMedia(cnt, $routeParams.create, $scope.files)
            .then(function (data) {
                $scope.content = data;
                notificationsService.success("Saved", "Media has been saved");
            });        
    };
}

angular.module("umbraco")
    .controller("Umbraco.Editors.MediaEditController", mediaEditController);