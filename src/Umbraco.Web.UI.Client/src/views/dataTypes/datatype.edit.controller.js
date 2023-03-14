/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataType.EditController
 * @function
 *
 * @description
 * The controller for the content editor
 */
function DataTypeEditController($scope, $routeParams, appState, navigationService, dataTypeResource, serverValidationManager, contentEditingHelper, formHelper, editorState, dataTypeHelper, eventsService, localizationService) {

    var evts = [];
    var vm = this;

    vm.header = {};
    vm.header.editorfor = "visuallyHiddenTexts_newDataType";
    vm.header.setPageTitle = true;

    //setup scope vars
    vm.page = {};
    vm.page.loading = false;
    vm.page.menu = {};
    vm.page.menu.currentSection = appState.getSectionState("currentSection");
    vm.page.menu.currentNode = null;

    //set up the standard data type props
    vm.properties = {
        selectedEditor: {
            alias: "selectedEditor",
            description: "Select a property editor",
            label: "Property editor",
            validation: {
                mandatory: true
            }
        }
    };

    //setup the pre-values as props
    vm.preValues = [];


    //method used to configure the pre-values when we retrieve them from the server
    function createPreValueProps(preVals) {
        vm.preValues = dataTypeHelper.createPreValueProps(preVals);
    }


    function setHeaderNameState(content) {
        if(content.isSystem == 1) {
            vm.page.nameLocked = true;
        }
    }


    function loadDataType() {

        vm.page.loading = true;
        vm.showIdentifier = true;

        //we are editing so get the content item from the server
        dataTypeResource.getById($routeParams.id)
            .then(function(data) {

                vm.preValuesLoaded = true;
                vm.content = data;

                createPreValueProps(vm.content.preValues);

                setHeaderNameState(vm.content);

                //share state
                editorState.set(vm.content);

                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.notifyAndClearAllSubscriptions();

                navigationService.syncTree({ tree: "dataTypes", path: data.path }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });

                vm.page.loading = false;

            });

    }

    function saveDataType() {

        if (formHelper.submitForm({ scope: $scope })) {

            vm.page.saveButtonState = "busy";

            dataTypeResource.save(vm.content, vm.preValues, $routeParams.create)
                .then(function(data) {

                    formHelper.resetForm({ scope: $scope });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        savedContent: data,
                        rebindCallback: function() {
                            createPreValueProps(data.preValues);
                        }
                    });

                    setHeaderNameState(vm.content);

                    //share state
                    editorState.set(vm.content);

                    navigationService.syncTree({ tree: "dataTypes", path: data.path, forceReload: true }).then(function (syncArgs) {
                        vm.page.menu.currentNode = syncArgs.node;
                    });

                    vm.page.saveButtonState = "success";

                    dataTypeHelper.rebindChangedProperties(vm.content, data);

                }, function(err) {

                    formHelper.resetForm({ scope: $scope, hasErrors: true });
                    //NOTE: in the case of data type values we are setting the orig/new props
                    // to be the same thing since that only really matters for content/media.
                    contentEditingHelper.handleSaveError({
                        err: err
                    });

                    vm.page.saveButtonState = "error";

                    //share state
                    editorState.set(vm.content);
                });
        }

    };

    vm.save = saveDataType;

    evts.push(eventsService.on("app.refreshEditor", function(name, error) {
        loadDataType();
    }));

    //ensure to unregister from all events!
    $scope.$on('$destroy', function () {
        for (var e in evts) {
            eventsService.unsubscribe(evts[e]);
        }
    });

    function init() {

        $scope.$watch("vm.content.selectedEditor", function (newVal, oldVal) {

            //when the value changes, we need to dynamically load in the new editor
            if (newVal && (newVal != oldVal && (oldVal || $routeParams.create))) {
                //we are editing so get the content item from the server
                var currDataTypeId = $routeParams.create ? undefined : $routeParams.id;
                dataTypeResource.getPreValues(newVal, currDataTypeId)
                    .then(function (data) {
                        vm.preValuesLoaded = true;
                        vm.content.preValues = data;
                        createPreValueProps(vm.content.preValues);

                        setHeaderNameState(vm.content);

                        //share state
                        editorState.set(vm.content);
                    });
            }
        });

        if ($routeParams.create) {

            vm.page.loading = true;
            vm.showIdentifier = false;

            //we are creating so get an empty data type item
            dataTypeResource.getScaffold($routeParams.id)
                .then(function(data) {

                    vm.preValuesLoaded = true;
                    vm.content = data;
                    vm.content.selectedEditor = null;

                    setHeaderNameState(vm.content);

                    //set a shared state
                    editorState.set(vm.content);

                    vm.page.loading = false;

                });
        }
        else {
            loadDataType();
        }

        var labelKeys = [
            "general_settings",
            "general_info"
        ];

        localizationService.localizeMany(labelKeys).then(function (values) {

            vm.page.navigation = [
                {
                    "name": values[0],
                    "alias": "settings",
                    "icon": "icon-settings",
                    "view": "views/dataTypes/views/datatype.settings.html",
                    "active": true
                },
                {
                    "name": values[1],
                    "alias": "info",
                    "icon": "icon-info",
                    "view": "views/dataTypes/views/datatype.info.html"
                }
            ];
        });
    }

    init();

}

angular.module("umbraco").controller("Umbraco.Editors.DataType.EditController", DataTypeEditController);
