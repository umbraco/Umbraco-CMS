function ContentEditDialogController($scope, $routeParams, $q, $timeout, $window, appState, contentResource, entityResource, navigationService, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, treeService, fileManager, formHelper, umbRequestHelper, umbModelMapper, $http) {
    //setup scope vars
    $scope.model = {};
    $scope.model.defaultButton = null;
    $scope.model.subButtons = [];
    $scope.model.nodeId = 0;

    var dialogOptions = $scope.$parent.dialogOptions;

    if(angular.isObject(dialogOptions.entity)){
        $scope.model.entity = $scope.filterTabs(dialogOptions.entity, dialogOptions.tabFilter);
        $scope.loaded = true;
    }else{

        if (dialogOptions.create) {
            //we are creating so get an empty content item
            contentResource.getScaffold(dialogOptions.id, dialogOptions.contentType)
                .then(function(data) {
                    $scope.loaded = true;
                    $scope.model.entity = $scope.filterTabs(data, dialogOptions.tabFilter);
                });
        }
        else {
            //we are editing so get the content item from the server
            contentResource.getById(dialogOptions.id)
                .then(function(data) {
                    $scope.loaded = true;
                    $scope.model.entity = $scope.filterTabs(data, dialogOptions.tabFilter);
                    
                    
                    //in one particular special case, after we've created a new item we redirect back to the edit
                    // route but there might be server validation errors in the collection which we need to display
                    // after the redirect, so we will bind all subscriptions which will show the server validation errors
                    // if there are any and then clear them so the collection no longer persists them.
                    serverValidationManager.executeAndClearAllSubscriptions();
                });
        }
    }

    function performSave(args) {
        var deferred = $q.defer();

        $scope.busy = true;

        if (formHelper.submitForm({ scope: $scope, statusMessage: args.statusMessage })) {

            args.saveMethod($scope.model.entity, $routeParams.create, fileManager.getFiles())
                .then(function (data) {

                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        savedContent: data,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.model.entity, data)
                    });

                    $scope.busy = false;
                    deferred.resolve(data);
                    
                }, function (err) {
                    
                    contentEditingHelper.handleSaveError({
                        redirectOnFailure: true,
                        err: err,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.model.entity, err.data)
                    });

                    $scope.busy = false;
                    deferred.reject(err);
                });
        }
        else {
            $scope.busy = false;
            deferred.reject();
        }

        return deferred.promise;
    }

    $scope.filterTabs = function(entity, blackList){
        if(blackList){
            _.each(entity.tabs, function(tab){
                tab.hide = _.contains(blackList, tab.alias);
            });
        }

        return entity;
    };

    $scope.saveAndPublish = function() {
        $scope.submit($scope.model.entity);
    };

    $scope.saveAndPublish = function() {
        performSave({ saveMethod: contentResource.publish, statusMessage: "Publishing..." })
            .then(function(content){
                if(dialogOptions.closeOnSave){
                    $scope.submit(content);
                }
            });
    };

    $scope.save = function () {
        performSave({ saveMethod: contentResource.save, statusMessage: "Saving..." })
            .then(function(content){
                if(dialogOptions.closeOnSave){
                    $scope.submit(content);
                }
            });
    };
}


angular.module("umbraco")
	.controller("Umbraco.Dialogs.Content.EditController", ContentEditDialogController);