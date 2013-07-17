/**
 * @ngdoc controller
 * @name ContentEditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function ContentEditController($scope, $routeParams, $location, contentResource, notificationsService, angularHelper, serverValidationService, contentEditingHelper) {
    
    //get the data to show, scaffold for new or get existing
    if ($routeParams.create) {
        contentResource.getScaffold($routeParams.id, $routeParams.doctype)
            .then(function(data) {
                $scope.contentLoaded = true;
                $scope.content = data;
            });
    }
    else {
        contentResource.getById($routeParams.id)
            .then(function(data) {
                $scope.contentLoaded = true;
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

    //ensure there is a form object assigned.
    var currentForm = angularHelper.getRequiredCurrentForm($scope);

    //TODO: Need to figure out a way to share the saving and event broadcasting with all editors!

    $scope.saveAndPublish = function (cnt) {

        $scope.$broadcast("saving", { scope: $scope });

        //don't continue if the form is invalid
        if (currentForm.$invalid) return;

        serverValidationService.reset();
        
        contentResource.publishContent(cnt, $routeParams.create, $scope.files)
            .then(function (data) {
                //TODO: only update the content that has changed!
                $scope.content = data;
                
                notificationsService.success("Published", "Content has been saved and published");                
                $scope.$broadcast("saved", { scope: $scope });

                contentEditingHelper.redirectToCreatedContent(data.id);
            }, function (err) {
                $location.search(null);
                //TODO: only update the content that has changed!
                $scope.content = err.data;
                contentEditingHelper.handleSaveError(err);
            });	        
    };

    $scope.save = function (cnt) {
	        
        $scope.$broadcast("saving", { scope: $scope });
            
        //don't continue if the form is invalid
        if (currentForm.$invalid) return;

        serverValidationService.reset();

        contentResource.saveContent(cnt, $routeParams.create, $scope.files)
            .then(function (data) {
                $scope.content = data;
                notificationsService.success("Saved", "Content has been saved");                
                $scope.$broadcast("saved", { scope: $scope });
            }, function (err) {
                //TODO: only update the content that has changed!
                $scope.content = err.data;
                contentEditingHelper.handleSaveError(err);
            });
	        
    };
}

angular.module("umbraco").controller("Umbraco.Editors.ContentEditController", ContentEditController);
