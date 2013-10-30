/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function ContentEditController($scope, $routeParams, $q, $timeout, $window, contentResource, navigationService, localizationService, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, fileManager, formHelper) {
    
    if ($routeParams.create) {
        //we are creating so get an empty content item
        contentResource.getScaffold($routeParams.id, $routeParams.doctype)
            .then(function(data) {
                $scope.loaded = true;
                $scope.content = data;
            });
    }
    else {
        //we are editing so get the content item from the server
        contentResource.getById($routeParams.id)
            .then(function(data) {
                $scope.loaded = true;
                $scope.content = data;

                //just get the cached version, no need to force a reload
                navigationService.syncPath(data.path.split(","), false);
                
                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.executeAndClearAllSubscriptions();


                contentResource.checkPermission("P", $routeParams.id).then(function(hasPermission){
                    var key = "buttons_sendToPublish"
                    if(hasPermission){
                        key = "buttons_saveAndPublish";
                    }
                    
                    localizationService.localize(key).then(function(label){
                        $scope.publishButtonLabel = label;
                    });
                });

            });
    }


    $scope.unPublish = function () {
        
        if (formHelper.submitForm({ scope: $scope, statusMessage: "Unpublishing...", skipValidation: true })) {

            contentResource.unPublish($scope.content.id)
                .then(function (data) {
                    
                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        newContent: data,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                    });

                    navigationService.syncPath(data.path.split(","), true);
                });
        }
        
    };

    $scope.saveAndPublish = function() {

        if (formHelper.submitForm({ scope: $scope, statusMessage: "Publishing..." })) {
            
            contentResource.publish($scope.content, $routeParams.create, fileManager.getFiles())
                .then(function(data) {

                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        newContent: data,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                    });

                    navigationService.syncPath(data.path.split(","), true);

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

    $scope.preview = function(content){
            if(!content.id){
                $scope.save().then(function(data){
                      $window.open('dialogs/preview.aspx?id='+data.id,'umbpreview');  
                });
            }else{
                $window.open('dialogs/preview.aspx?id='+content.id,'umbpreview');
            }    
    };

    $scope.save = function() {
        var deferred = $q.defer();

        if (formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {

            contentResource.save($scope.content, $routeParams.create, fileManager.getFiles())
                .then(function(data) {

                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        newContent: data,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                    });

                    //fetch tree
                    navigationService.syncPath(data.path.split(","), true);

                    deferred.resolve(data);
                }, function(err) {
                    contentEditingHelper.handleSaveError({
                        err: err,
                        allNewProps: contentEditingHelper.getAllProps(err.data),
                        allOrigProps: contentEditingHelper.getAllProps($scope.content)
                    });

                    deferred.reject(err);
                });
        }
        else {
            deferred.reject();
        }

        return deferred.promise;
    };

}

angular.module("umbraco").controller("Umbraco.Editors.Content.EditController", ContentEditController);
