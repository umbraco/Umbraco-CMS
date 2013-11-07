/**
 * @ngdoc controller
 * @name Umbraco.Editors.Media.EditController
 * @function
 * 
 * @description
 * The controller for the media editor
 */
function mediaEditController($scope, $routeParams, mediaResource, navigationService, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, fileManager, treeService,  formHelper) {

    $scope.nav = navigationService;
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
                navigationService.syncPath(data.path, true);
            });
    }
    
    $scope.options = function(content){
            if(!content.id){
                return;
            }

            if(!$scope.actions){
                treeService.getMenu({ treeNode: $scope.nav.ui.currentNode })
                    .then(function(data) {
                            $scope.actions = data.menuItems;
                    });    
            }
        };

    $scope.save = function () {

        if (formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {

            mediaResource.save($scope.content, $routeParams.create, fileManager.getFiles())
                .then(function(data) {

                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        newContent: data,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                    });

                    navigationService.syncPath(data.path, true);
                }, function(err) {

                    contentEditingHelper.handleSaveError({
                        err: err,
                        redirectOnFailure: true,
                        allNewProps: contentEditingHelper.getAllProps(err.data),
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, err.data)
                    });
                });
        }
        
    };
}

angular.module("umbraco")
    .controller("Umbraco.Editors.Media.EditController", mediaEditController);