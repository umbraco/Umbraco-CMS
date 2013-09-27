/**
 * @ngdoc controller
 * @name Umbraco.Editors.Media.EditController
 * @function
 * 
 * @description
 * The controller for the media editor
 */
function mediaEditController($scope, $routeParams, mediaResource, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, fileManager, editorContextService, $timeout) {

    //initialize the file manager
    fileManager.clearFiles();

    if ($routeParams.create) {

        mediaResource.getScaffold($routeParams.id, $routeParams.doctype)
            .then(function (data) {
                $scope.loaded = true;
                $scope.content = data;

                editorContextService.setContext($scope.content);
            });
    }
    else {
        mediaResource.getById($routeParams.id)
            .then(function (data) {
                $scope.loaded = true;
                $scope.content = data;
                
                editorContextService.setContext($scope.content);
                
                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.executeAndClearAllSubscriptions();

            });
    }
    
    $scope.setStatus = function(status){
        //add localization
        $scope.status = status;
        $timeout(function(){
            $scope.status = undefined;
        }, 2500);
    };

    $scope.save = function () {
        
        $scope.setStatus("Saving...");
        
        $scope.$broadcast("saving", { scope: $scope });

        var currentForm = angularHelper.getRequiredCurrentForm($scope);
        //don't continue if the form is invalid
        if (currentForm.$invalid) return;
        
        serverValidationManager.reset();

        mediaResource.save($scope.content, $routeParams.create, fileManager.getFiles())
            .then(function (data) {

                contentEditingHelper.handleSuccessfulSave({
                    scope: $scope,
                    newContent: data,
                    rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                });
                
            }, function (err) {
                
                contentEditingHelper.handleSaveError({
                    err: err,
                    redirectOnFailure: true,
                    allNewProps: contentEditingHelper.getAllProps(err.data),
                    rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, err.data)
                });
            });
    };
}

angular.module("umbraco")
    .controller("Umbraco.Editors.Media.EditController", mediaEditController);