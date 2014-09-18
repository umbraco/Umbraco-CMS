/**
 * @ngdoc controller
 * @name Umbraco.Editors.Member.ListController
 * @function
 * 
 * @description
 * The controller for the member list view
 */
function MemberListController($scope, $routeParams, $location, $q, $window, appState, memberResource, entityResource, navigationService, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, fileManager, formHelper, umbModelMapper, editorState) {
    
    //setup scope vars
    $scope.currentSection = appState.getSectionState("currentSection");
    $scope.currentNode = null; //the editors affiliated node

    //build a path to sync the tree with
    function buildTreePath(data) {
        var path = data.name[0].toLowerCase() + "," + data.key;
        return path;
    }

    //we are editing so get the content item from the server
    memberResource.getListNode($routeParams.id)
        .then(function (data) {
            $scope.loaded = true;
            $scope.content = data;

            editorState.set($scope.content);

            var path = buildTreePath(data);

            navigationService.syncTree({ tree: "member", path: path.split(",") }).then(function (syncArgs) {
                $scope.currentNode = syncArgs.node;
            });

            //in one particular special case, after we've created a new item we redirect back to the edit
            // route but there might be server validation errors in the collection which we need to display
            // after the redirect, so we will bind all subscriptions which will show the server validation errors
            // if there are any and then clear them so the collection no longer persists them.
            serverValidationManager.executeAndClearAllSubscriptions();
        });
}

angular.module("umbraco").controller("Umbraco.Editors.Member.ListController", MemberListController);
