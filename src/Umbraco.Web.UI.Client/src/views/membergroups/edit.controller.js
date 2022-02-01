/**
 * @ngdoc controller
 * @name Umbraco.Editors.MemberGroups.EditController
 * @function
 *
 * @description
 * The controller for the member group editor
 */
function MemberGroupsEditController($scope, $routeParams, appState, navigationService, memberGroupResource, contentEditingHelper, formHelper, editorState, eventsService) {

    //setup scope vars
    $scope.page = {};
    $scope.page.loading = false;
    $scope.header = {};
    $scope.header.editorfor = "content_membergroup";
    $scope.header.setPageTitle = true;
    $scope.page.menu = {};
    $scope.page.menu.currentSection = appState.getSectionState("currentSection");
    $scope.page.menu.currentNode = null;
    var evts = [];

    if ($routeParams.create) {

        $scope.page.loading = true;

        //we are creating so get an empty member group item
        memberGroupResource.getScaffold()
            .then(function(data) {

                $scope.content = data;

                //set a shared state
                editorState.set($scope.content);

                $scope.page.loading = false;

            });
    }
    else {
        loadMemberGroup();
    }

    function loadMemberGroup() {

        $scope.page.loading = true;

        //we are editing so get the content item from the server
        memberGroupResource.getById($routeParams.id)
            .then(function (data) {
                $scope.content = data;

                //share state
                editorState.set($scope.content);

                navigationService.syncTree({ tree: "memberGroups", path: data.path }).then(function (syncArgs) {
                    $scope.page.menu.currentNode = syncArgs.node;
                });

                $scope.page.loading = false;

            });
    }

    $scope.save = function () {

        if (formHelper.submitForm({ scope: $scope })) {

            $scope.page.saveButtonState = "busy";

            memberGroupResource.save($scope.content, $scope.preValues, $routeParams.create)
                .then(function (data) {

                    formHelper.resetForm({ scope: $scope });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        savedContent: data
                    });

                    //share state
                    editorState.set($scope.content);

                    navigationService.syncTree({ tree: "memberGroups", path: data.path, forceReload: true }).then(function (syncArgs) {
                        $scope.page.menu.currentNode = syncArgs.node;
                    });

                    $scope.page.saveButtonState = "success";

                }, function (err) {

                    formHelper.resetForm({ scope: $scope, hasErrors: true });
                    contentEditingHelper.handleSaveError({
                        err: err
                    });

                    $scope.page.saveButtonState = "error";

                    //share state
                    editorState.set($scope.content);
                });
        }

    };

    evts.push(eventsService.on("app.refreshEditor", function (name, error) {
        loadMemberGroup();
    }));

    //ensure to unregister from all events!
    $scope.$on('$destroy', function () {
        for (var e in evts) {
            eventsService.unsubscribe(evts[e]);
        }
    });

}

angular.module("umbraco").controller("Umbraco.Editors.MemberGroups.EditController", MemberGroupsEditController);
