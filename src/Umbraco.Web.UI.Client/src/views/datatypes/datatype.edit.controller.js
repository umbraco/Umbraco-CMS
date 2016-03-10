/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataType.EditController
 * @function
 *
 * @description
 * The controller for the content editor
 */
function DataTypeEditController($scope, $routeParams, $location, appState, navigationService, treeService, dataTypeResource, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, formHelper, editorState, dataTypeHelper, eventsService) {

    //setup scope vars
    $scope.page = {};
    $scope.page.loading = false;
    $scope.page.nameLocked = false;
    $scope.page.menu = {};
    $scope.page.menu.currentSection = appState.getSectionState("currentSection");
    $scope.page.menu.currentNode = null;
    var evts = [];

    //method used to configure the pre-values when we retrieve them from the server
    function createPreValueProps(preVals) {
        $scope.preValues = [];
        for (var i = 0; i < preVals.length; i++) {
            $scope.preValues.push({
                hideLabel: preVals[i].hideLabel,
                alias: preVals[i].key,
                description: preVals[i].description,
                label: preVals[i].label,
                view: preVals[i].view,
                value: preVals[i].value
            });
        }
    }

    //set up the standard data type props
    $scope.properties = {
        selectedEditor: {
            alias: "selectedEditor",
            description: "Select a property editor",
            label: "Property editor"
        },
        selectedEditorId: {
            alias: "selectedEditorId",
            label: "Property editor alias"
        }
    };

    //setup the pre-values as props
    $scope.preValues = [];

    if ($routeParams.create) {

        $scope.page.loading = true;

        //we are creating so get an empty data type item
        dataTypeResource.getScaffold($routeParams.id)
            .then(function(data) {

                $scope.preValuesLoaded = true;
                $scope.content = data;

                setHeaderNameState($scope.content);

                //set a shared state
                editorState.set($scope.content);

                $scope.page.loading = false;

            });
    }
    else {
        loadDataType();
    }

    function loadDataType() {

        $scope.page.loading = true;

        //we are editing so get the content item from the server
        dataTypeResource.getById($routeParams.id)
            .then(function(data) {

                $scope.preValuesLoaded = true;
                $scope.content = data;

                createPreValueProps($scope.content.preValues);

                setHeaderNameState($scope.content);

                //share state
                editorState.set($scope.content);

                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.executeAndClearAllSubscriptions();

                navigationService.syncTree({ tree: "datatypes", path: data.path }).then(function (syncArgs) {
                    $scope.page.menu.currentNode = syncArgs.node;
                });

                $scope.page.loading = false;

            });
    }

    $scope.$watch("content.selectedEditor", function (newVal, oldVal) {

        //when the value changes, we need to dynamically load in the new editor
        if (newVal && (newVal != oldVal && (oldVal || $routeParams.create))) {
            //we are editing so get the content item from the server
            var currDataTypeId = $routeParams.create ? undefined : $routeParams.id;
            dataTypeResource.getPreValues(newVal, currDataTypeId)
                .then(function (data) {
                    $scope.preValuesLoaded = true;
                    $scope.content.preValues = data;
                    createPreValueProps($scope.content.preValues);

                    setHeaderNameState($scope.content);

                    //share state
                    editorState.set($scope.content);
                });
        }
    });

    function setHeaderNameState(content) {

      if(content.isSystem == 1) {
         $scope.page.nameLocked = true;
      }

    }

    $scope.save = function() {

        if (formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {

            $scope.page.saveButtonState = "busy";

            dataTypeResource.save($scope.content, $scope.preValues, $routeParams.create)
                .then(function(data) {

                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        savedContent: data,
                        rebindCallback: function() {
                            createPreValueProps(data.preValues);
                        }
                    });

                    setHeaderNameState($scope.content);

                    //share state
                    editorState.set($scope.content);

                    navigationService.syncTree({ tree: "datatypes", path: data.path, forceReload: true }).then(function (syncArgs) {
                        $scope.page.menu.currentNode = syncArgs.node;
                    });

                    $scope.page.saveButtonState = "success";

                    dataTypeHelper.rebindChangedProperties($scope.content, data);

                }, function(err) {

                    //NOTE: in the case of data type values we are setting the orig/new props
                    // to be the same thing since that only really matters for content/media.
                    contentEditingHelper.handleSaveError({
                        redirectOnFailure: false,
                        err: err
                    });

                    $scope.page.saveButtonState = "error";

                    //share state
                    editorState.set($scope.content);

                    dataTypeHelper.rebindChangedProperties($scope.content, data);
                });
        }

    };

    evts.push(eventsService.on("app.refreshEditor", function(name, error) {
        loadDataType();
    }));

    //ensure to unregister from all events!
    $scope.$on('$destroy', function () {
        for (var e in evts) {
            eventsService.unsubscribe(evts[e]);
        }
    });

}

angular.module("umbraco").controller("Umbraco.Editors.DataType.EditController", DataTypeEditController);
