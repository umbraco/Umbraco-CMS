(function () {
    'use strict';

    function ListViewSettingsDirective(dataTypeResource, dataTypeHelper, editorService, listViewPrevalueHelper) {

        function link(scope) {

            scope.dataType = {};
            scope.editDataTypeSettings = false;
            scope.customListViewCreated = false;

            /* ---------- INIT ---------- */

            function activate() {

                if (scope.enableListView) {

                    dataTypeResource.getByName(scope.listViewName)
                        .then(dataType => {

                            scope.dataType = dataType;

                            listViewPrevalueHelper.setPrevalues(dataType.preValues);
                            scope.customListViewCreated = checkForCustomListView();
                        });

                } else {
                    scope.dataType = {};
                }

            }

            /* ----------- LIST VIEW SETTINGS --------- */
            scope.toggleEditListViewDataTypeSettings = function () {
                const overlay = {
                    view: 'views/components/umb-list-view-settings-overlay.html',
                    hideDescription: true,
                    hideIcon: true,
                    dataType: scope.dataType,
                    title: 'List view settings',
                    submit: model => {
                        scope.dataType = model.dataType;
                        const preValues = dataTypeHelper.createPreValueProps(scope.dataType.preValues);

                        dataTypeResource.save(scope.dataType, preValues, false).then(dataType => {
                            // store data type
                            scope.dataType = dataType;
                        });
                        
                        editorService.close();
                    },
                    close: () => editorService.close()
                };

                editorService.open(overlay);
            };


            /* ---------- CUSTOM LIST VIEW ---------- */

            scope.createCustomListViewDataType = function () {

                dataTypeResource.createCustomListView(scope.modelAlias).then(function (dataType) {

                    // store data type
                    scope.dataType = dataType;

                    // set list view name on scope
                    scope.listViewName = dataType.name;

                    // change state to custom list view
                    scope.customListViewCreated = true;

                    // show settings overlay
                    scope.toggleEditListViewDataTypeSettings();

                });

            };

            scope.removeCustomListDataType = function () {

                // delete custom list view data type
                dataTypeResource.deleteById(scope.dataType.id).then(function (dataType) {

                    // set list view name on scope
                    if (scope.contentType === "documentType") {

                        scope.listViewName = "List View - Content";

                    } else if (scope.contentType === "mediaType") {

                        scope.listViewName = "List View - Media";

                    }

                    // get default data type
                    dataTypeResource.getByName(scope.listViewName)
                        .then(function (dataType) {

                            // store data type
                            scope.dataType = dataType;

                            // change state to default list view
                            scope.customListViewCreated = false;

                        });
                });

            };

            scope.toggle = function () {
                if (scope.enableListView) {
                    scope.enableListView = false;
                    return;
                }
                scope.enableListView = true;
            };

            /* ----------- SCOPE WATCHERS ----------- */
            var unbindEnableListViewWatcher = scope.$watch('enableListView', function (newValue) {

                if (newValue !== undefined) {
                    activate();
                }

            });

            // clean up
            scope.$on('$destroy', function () {
                unbindEnableListViewWatcher();
            });

            /* ----------- METHODS ---------- */

            function checkForCustomListView() {
                return scope.dataType.name === "List View - " + scope.modelAlias;
            }

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-list-view-settings.html',
            scope: {
                enableListView: "=",
                listViewName: "=",
                modelAlias: "=",
                contentType: "@"
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbListViewSettings', ListViewSettingsDirective);

})();
