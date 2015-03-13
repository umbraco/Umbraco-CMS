/**
 * @ngdoc controller
 * @name Umbraco.Editors.Dictionary.EditController
 * @function
 * 
 * @description
 * The controller for the dictionary editor
 */
function DictionaryEditController($scope, $routeParams, appState, navigationService, dictionaryResource, serverValidationManager, contentEditingHelper, formHelper, editorState) {

    //setup scope vars    
    $scope.currentSection = appState.getSectionState("currentSection");
    $scope.currentNode = null; //the editors affiliated node

    if ($routeParams.create) {
        //we are creating so get an empty dictionary item
        dictionaryResource.getScaffold()
            .then(function(data) {
                $scope.loaded = true;
                $scope.content = data;

                console.info(data);

                //set a shared state
                editorState.set($scope.content);
            });
    }
    else {
        //we are editing so get the dictionary item from the server
        dictionaryResource.getById($routeParams.id)
            .then(function (data) {
                $scope.loaded = true;
                $scope.content = data;

                console.info(data);
                // TODO: [LK] Implement a callback (to populate the translations?)

                //share state
                editorState.set($scope.content);

                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.executeAndClearAllSubscriptions();
                
                navigationService.syncTree({ tree: "dictionary", path: [String(data.id)] }).then(function (syncArgs) {
                    $scope.currentNode = syncArgs.node;
                });
            });
    }

    $scope.save = function() {

        if (formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {

            dictionaryResource.save($scope.content, $scope.translations, $routeParams.create)
                .then(function(data) {

                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        savedContent: data,
                        rebindCallback: function() {
                             // TODO: [LK] Implement a callback (to populate the translations?)
                        }
                    });

                    //share state
                    editorState.set($scope.content);

                    navigationService.syncTree({ tree: "dictionary", path: [String(data.id)], forceReload: true }).then(function (syncArgs) {
                        $scope.currentNode = syncArgs.node;
                    });
                    
                }, function(err) {

                    //NOTE: in the case of data type values we are setting the orig/new props 
                    // to be the same thing since that only really matters for content/media.
                    contentEditingHelper.handleSaveError({
                        redirectOnFailure: false,
                        err: err
                    });
                    
                    //share state
                    editorState.set($scope.content);
                });
        }

    };

}

angular.module("umbraco").controller("Umbraco.Editors.Dictionary.EditController", DictionaryEditController);
