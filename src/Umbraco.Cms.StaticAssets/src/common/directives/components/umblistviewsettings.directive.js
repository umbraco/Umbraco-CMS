(function () {
    'use strict';

    function ListViewSettingsDirective(dataTypeResource, dataTypeHelper, editorService, listViewPrevalueHelper) {

        function link(scope) {

            scope.dataType = {};
            scope.customListViewCreated = false;

            const listViewPrefix = "List View - ";
            
            const checkForCustomListView = () => invariantEquals(scope.dataType.name, listViewPrefix + scope.modelAlias);

            // We also use "localeCompare" a few other places. Should probably be moved to a utility/helper function in future.
            function invariantEquals(a, b) {
                return typeof a === "string" && typeof b === "string"
                    ? a.localeCompare(b, undefined, { sensitivity: "base" }) === 0
                    : a === b;
            }

            /* ---------- INIT ---------- */ 

            const setDataType = (dataType) => {
                scope.dataType = dataType;
                listViewPrevalueHelper.setPrevalues(dataType.preValues);
            }

            const activate = () => {

                if (scope.enableListView) {

                    dataTypeResource.getByName(scope.listViewName)
                        .then(dataType => {
                            setDataType(dataType);
                            scope.customListViewCreated = checkForCustomListView();
                        });

                } else {
                    scope.dataType = {};
                } 
            } 

            /* ----------- LIST VIEW SETTINGS --------- */
            const showSettingsOverlay = () => {
                const overlay = {
                    view: 'views/components/umb-list-view-settings-overlay.html',
                    hideDescription: true,
                    hideIcon: true,
                    size: 'medium',
                    dataType: scope.dataType,
                    title: 'List view settings',
                    submit: model => {
                        const preValues = dataTypeHelper.createPreValueProps(model.dataType.preValues);

                        // store data type   
                        dataTypeResource.save(model.dataType, preValues, false)
                            .then(dataType => scope.dataType = dataType);                      
                        
                        editorService.close();
                    },
                    close: () => editorService.close()
                };

                editorService.open(overlay);
            };


            /* ---------- CUSTOM LIST VIEW ---------- */

            scope.createCustomListViewDataType = () => {

                scope.loading = true;
                
                dataTypeResource.createCustomListView(scope.modelAlias).then(dataType => {

                    // store data type
                    setDataType(dataType);

                    // set list view name on scope
                    scope.listViewName = dataType.name;

                    // change state to custom list view
                    scope.customListViewCreated = true;

                    // show settings overlay
                    showSettingsOverlay();
                    
                    scope.loading = false;

                }); 
            }; 

            scope.removeCustomListDataType = () => {
                
                scope.loading = true;

                // delete custom list view data type
                dataTypeResource.deleteById(scope.dataType.id).then(dataType => {

                    // set list view name on scope
                    scope.listViewName = `List View - ${scope.contentType === 'documentType' ? 'Content' : 'Media'}`;

                    // get default data type 
                    dataTypeResource.getByName(scope.listViewName)
                        .then(defaultDataType => {

                            // store data type
                            setDataType(defaultDataType);

                            // change state to default list view
                            scope.customListViewCreated = false;
                            
                            scope.loading = false;
                        });
                });
            };

            scope.toggle = () => scope.enableListView = !scope.enableListView;           
            scope.showSettingsOverlay = () => showSettingsOverlay();
            
            
            /* ----------- SCOPE WATCHERS ----------- */
            const unbindEnableListViewWatcher = scope.$watch('enableListView', newValue => {

                if (newValue !== undefined) {
                    activate();
                }

            });

            // clean up
            scope.$on('$destroy', () => unbindEnableListViewWatcher());
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
