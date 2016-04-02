/**
 * @ngdoc controller
 * @name Umbraco.Editors.UserTypes.EditController
 * @function
 *
 * @description
 * The controller for the user types editor
 */
function UserTypesEditController($scope, $routeParams, $location, $q, $window, localizationService, appState, eventsService, userTypeResource, entityResource, navigationService, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, fileManager, formHelper, umbModelMapper, editorState, umbRequestHelper, $http) {
    //setup scope vars
    $scope.page = {};
    $scope.page.loading = false;
    $scope.page.menu = {};
    $scope.page.menu.currentSection = appState.getSectionState("currentSection");
    $scope.page.menu.currentNode = null;
    $scope.permissions = [];
    var evts = [];

    if ($routeParams.create) {

        $scope.page.loading = true;

        userTypeResource.getScaffold()
            .then(function (data) {
                init(data);
                

            });
    }
    else {
        loadUserType();
    }

    function init(data) {
        userTypeResource.getPermissions().then(function (permissions) {
            var items = [];
            for (var i in permissions) {
                var permission = permissions[i];
                console.log(permission.key)
                items.push({ "id": permission.key, "value": "actions_" + permission.name, localize: true });
            }
            $scope.content = data;
            var defaultPermissionsProperty = {
                "label": null,
                "description": null,
                "view": "checkboxlist",
                "config": { items: items },
                "hideLabel": false,
                "validation": { "mandatory": false, "pattern": null },
                "id": 1,
                "value": data.permissions,
                "alias": "defaultPermissions",
                "editor": "Umbraco.CheckBoxList"
            }
            $scope.defaultPermissionsProperty = defaultPermissionsProperty;
            //set a shared state
            editorState.set($scope.content);

            /*navigationService.syncTree({ tree: "userTypes", path: data.path }).then(function (syncArgs) {
                    //$scope.page.menu.currentNode = syncArgs.node;
                });*/

            $scope.page.loading = false;
        });
        
    }

    function loadUserType() {

        $scope.page.loading = true;

        //we are editing so get the content item from the server
        userTypeResource.getById($routeParams.id)
            .then(function (data) {
                init(data);
            });
    }

    $scope.save = function () {

        if (formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {

            $scope.page.saveButtonState = "busy";

            $scope.content.permissions = $scope.defaultPermissionsProperty.value;
            userTypeResource.save($scope.content, $routeParams.create)
                .then(function (data) {

                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        savedContent: data
                    });

                    //share state
                    editorState.set($scope.content);

                    /*navigationService.syncTree({ tree: "memberGroups", path: data.path, forceReload: true }).then(function (syncArgs) {
                        $scope.page.menu.currentNode = syncArgs.node;
                    });*/

                    $scope.page.saveButtonState = "success";

                }, function (err) {

                    contentEditingHelper.handleSaveError({
                        redirectOnFailure: false,
                        err: err
                    });

                    $scope.page.saveButtonState = "error";

                    //share state
                    editorState.set($scope.content);
                });
        }

    };

    evts.push(eventsService.on("app.refreshEditor", function (name, error) {
        loadUserType();
    }));

    //ensure to unregister from all events!
    $scope.$on('$destroy', function () {
        for (var e in evts) {
            eventsService.unsubscribe(evts[e]);
        }
    });

}

angular.module("umbraco").controller("Umbraco.Editors.UserTypes.EditController", UserTypesEditController);
