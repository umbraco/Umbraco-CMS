/**
 * @ngdoc controller
 * @name Umbraco.Editors.Media.EditController
 * @function
 * 
 * @description
 * The controller for the media editor
 */
function mediaEditController($scope, $routeParams, mediaResource, notificationsService, angularHelper, serverValidationManager, contentEditingHelper) {

    if ($routeParams.create) {

        mediaResource.getScaffold($routeParams.id, $routeParams.doctype)
            .then(function (data) {
                $scope.loaded = true;
                $scope.content = data;
            });
    }
    else {
        mediaResource.getById($routeParams.id)
            .then(function (data) {
                $scope.loaded = true;
                $scope.content = data;
                
                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.executeAndClearAllSubscriptions();

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
        
    $scope.save = function () {
        
        $scope.$broadcast("saving", { scope: $scope });

        var currentForm = angularHelper.getRequiredCurrentForm($scope);
        //don't continue if the form is invalid
        if (currentForm.$invalid) return;
        
        serverValidationManager.reset();

        mediaResource.save($scope.content, $routeParams.create, $scope.files)
            .then(function (data) {

                contentEditingHelper.handleSuccessfulSave({
                    scope: $scope,
                    newContent: data,
                    rebindCallback: contentEditingHelper.reBindChangedProperties(
                        contentEditingHelper.getAllProps($scope.content),
                        contentEditingHelper.getAllProps(data))
                });
                
            }, function (err) {
                
                var allNewProps = contentEditingHelper.getAllProps(err.data);
                var allOrigProps = contentEditingHelper.getAllProps($scope.content);

                contentEditingHelper.handleSaveError({
                    err: err,
                    redirectOnFailure: true,
                    allNewProps: allNewProps,
                    allOrigProps: contentEditingHelper.getAllProps($scope.content),
                    rebindCallback: contentEditingHelper.reBindChangedProperties(allOrigProps, allNewProps)
                });
            });
    };
}

angular.module("umbraco")
    .controller("Umbraco.Editors.Media.EditController", mediaEditController);