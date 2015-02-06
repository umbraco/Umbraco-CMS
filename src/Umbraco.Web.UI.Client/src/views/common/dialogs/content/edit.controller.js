function ContentEditDialogController($scope, editorState, $routeParams, $q, $timeout, $window, appState, contentResource, entityResource, navigationService, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, treeService, fileManager, formHelper, umbRequestHelper, umbModelMapper, $http) {
    
    $scope.defaultButton = null;
    $scope.subButtons = [];
    var dialogOptions = $scope.$parent.dialogOptions;

    // This is a helper method to reduce the amount of code repitition for actions: Save, Publish, SendToPublish
    function performSave(args) {
        contentEditingHelper.contentEditorPerformSave({
            statusMessage: args.statusMessage,
            saveMethod: args.saveMethod,
            scope: $scope,
            content: $scope.content
        }).then(function (content) {
            //success            
            if (dialogOptions.closeOnSave) {
                $scope.submit(content);
            }

        }, function(err) {
            //error
        });
    }

    function filterTabs(entity, blackList) {
        if (blackList) {
            _.each(entity.tabs, function (tab) {
                tab.hide = _.contains(blackList, tab.alias);
            });
        }

        return entity;
    };
    
    function init(content) {
        var buttons = contentEditingHelper.configureContentEditorButtons({
            create: $routeParams.create,
            content: content,
            methods: {
                saveAndPublish: $scope.saveAndPublish,
                sendToPublish: $scope.sendToPublish,
                save: $scope.save,
                unPublish: angular.noop
            }
        });
        $scope.defaultButton = buttons.defaultButton;
        $scope.subButtons = buttons.subButtons;

        //This is a total hack but we have really no other way of sharing data to the property editors of this
        // content item, so we'll just set the property on the content item directly
        $scope.content.isDialogEditor = true;

        editorState.set($scope.content);
    }

    //check if the entity is being passed in, otherwise load it from the server
    if (angular.isObject(dialogOptions.entity)) {
        $scope.loaded = true;
        $scope.content = filterTabs(dialogOptions.entity, dialogOptions.tabFilter);
        init($scope.content);
    }
    else {
        contentResource.getById(dialogOptions.id)
            .then(function(data) {
                $scope.loaded = true;
                $scope.content = filterTabs(data, dialogOptions.tabFilter);
                init($scope.content);
                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.executeAndClearAllSubscriptions();
            });
    }  

    $scope.sendToPublish = function () {
        performSave({ saveMethod: contentResource.sendToPublish, statusMessage: "Sending..." });
    };

    $scope.saveAndPublish = function () {
        performSave({ saveMethod: contentResource.publish, statusMessage: "Publishing..." });
    };

    $scope.save = function () {
        performSave({ saveMethod: contentResource.save, statusMessage: "Saving..." });
    };

    // this method is called for all action buttons and then we proxy based on the btn definition
    $scope.performAction = function (btn) {

        if (!btn || !angular.isFunction(btn.handler)) {
            throw "btn.handler must be a function reference";
        }

        if (!$scope.busy) {
            btn.handler.apply(this);
        }
    };

}


angular.module("umbraco")
	.controller("Umbraco.Dialogs.Content.EditController", ContentEditDialogController);