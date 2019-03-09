/**
 * @ngdoc controller
 * @name Umbraco.Editors.Member.EditController
 * @function
 *
 * @description
 * The controller for the member editor
 */
function MemberEditController($scope, $routeParams, $location, appState, memberResource, entityResource, navigationService, notificationsService, localizationService, serverValidationManager, contentEditingHelper, fileManager, formHelper, editorState, umbRequestHelper, $http) {

    //setup scope vars
    $scope.page = {};
    $scope.page.loading = true;
    $scope.page.menu = {};
    $scope.page.menu.currentSection = appState.getSectionState("currentSection");
    $scope.page.menu.currentNode = null; //the editors affiliated node
    $scope.page.nameLocked = false;
    $scope.page.listViewPath = null;
    $scope.page.saveButtonState = "init";
    $scope.page.exportButton = "init";

    $scope.page.listViewPath = ($routeParams.page && $routeParams.listName)
        ? "/member/member/list/" + $routeParams.listName + "?page=" + $routeParams.page
        : null;

    //build a path to sync the tree with
    function buildTreePath(data) {
        return $routeParams.listName ? "-1," + $routeParams.listName : "-1";
    }

    if ($routeParams.create) {

        //if there is no doc type specified then we are going to assume that
        // we are not using the umbraco membership provider
        if ($routeParams.doctype) {

            //we are creating so get an empty member item
            memberResource.getScaffold($routeParams.doctype)
                .then(function(data) {

                    $scope.content = data;

                    setHeaderNameState($scope.content);

                    editorState.set($scope.content);

                    $scope.page.loading = false;

                });
        }
        else {

            memberResource.getScaffold()
                .then(function (data) {
                    $scope.content = data;

                    setHeaderNameState($scope.content);

                    editorState.set($scope.content);

                    $scope.page.loading = false;

                });
        }

    }
    else {
        //so, we usually refernce all editors with the Int ID, but with members we have
        //a different pattern, adding a route-redirect here to handle this just in case.
        //(isNumber doesnt work here since its seen as a string)
        //The reason this might be an INT is due to the routing used for the member list view
        //but this is now configured to use the key, so this is just a fail safe

        if ($routeParams.id && $routeParams.id.length < 9) {

            entityResource.getById($routeParams.id, "Member").then(function(entity) {
                $location.path("/member/member/edit/" + entity.key);
            });
        }
        else {

            //we are editing so get the content item from the server
            memberResource.getByKey($routeParams.id)
                .then(function(data) {

                    $scope.content = data;

                    setHeaderNameState($scope.content);

                    editorState.set($scope.content);

                    var path = buildTreePath(data);

                    //sync the tree (only for ui purposes)
                    navigationService.syncTree({ tree: "member", path: path.split(",") });

                    //it's the initial load of the editor, we need to get the tree node
                    // from the server so that we can load in the actions menu.
                    umbRequestHelper.resourcePromise(
                        $http.get(data.treeNodeUrl),
                        'Failed to retrieve data for child node ' + data.key).then(function (node) {
                            $scope.page.menu.currentNode = node;
                        });

                    //in one particular special case, after we've created a new item we redirect back to the edit
                    // route but there might be server validation errors in the collection which we need to display
                    // after the redirect, so we will bind all subscriptions which will show the server validation errors
                    // if there are any and then clear them so the collection no longer persists them.
                    serverValidationManager.notifyAndClearAllSubscriptions();

                    $scope.page.loading = false;

                });
        }

    }

    function setHeaderNameState(content) {

      if(content.membershipScenario === 0) {
         $scope.page.nameLocked = true;
      }
    }

    /** Just shows a simple notification that there are client side validation issues to be fixed */
    function showValidationNotification() {
        //TODO: We need to make the validation UI much better, there's a lot of inconsistencies in v8 including colors, issues with the property groups and validation errors between variants

        //need to show a notification else it's not clear there was an error.
        localizationService.localizeMany([
                "speechBubbles_validationFailedHeader",
                "speechBubbles_validationFailedMessage"
            ]
        ).then(function (data) {
            notificationsService.error(data[0], data[1]);
        });
    }

    $scope.save = function() {

        if (formHelper.submitForm({ scope: $scope })) {

            $scope.page.saveButtonState = "busy";

            memberResource.save($scope.content, $routeParams.create, fileManager.getFiles())
                .then(function(data) {

                    formHelper.resetForm({ scope: $scope });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        savedContent: data,
                        //specify a custom id to redirect to since we want to use the GUID
                        redirectId: data.key,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                    });

                    editorState.set($scope.content);
                    $scope.page.saveButtonState = "success";

                    var path = buildTreePath(data);

                    //sync the tree (only for ui purposes)
                    navigationService.syncTree({ tree: "member", path: path.split(","), forceReload: true });

            }, function (err) {

                    contentEditingHelper.handleSaveError({
                        redirectOnFailure: false,
                        err: err,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, err.data)
                    });

                    editorState.set($scope.content);
                    $scope.page.saveButtonState = "error";

                });
        }
        else {
            showValidationNotification();
        }

    };

    $scope.export = function() {
        var memberKey = $scope.content.key;
        memberResource.exportMemberData(memberKey);
    }

}

angular.module("umbraco").controller("Umbraco.Editors.Member.EditController", MemberEditController);
