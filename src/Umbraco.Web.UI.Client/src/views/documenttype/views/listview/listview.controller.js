/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.ListViewController
 * @function
 *
 * @description
 * The controller for the content type editor list view section
 */
(function() {
    'use strict';

    function ListViewController($scope, contentTypeResource, dataTypeResource, dataTypeHelper) {

        /* ---------- SCOPE VARIABLES ---------- */

        var vm = this;

        vm.toggleListView = toggleListView;
        vm.toggleEditListViewDataTypeSettings = toggleEditListViewDataTypeSettings;
        vm.saveListViewDataType = saveListViewDataType;
        vm.createCustomListViewDataType = createCustomListViewDataType;
        vm.removeCustomListDataType = removeCustomListDataType;

        vm.dataType = {};
        vm.editDataTypeSettings = false;
        vm.customListViewCreated = false;


        /* ---------- INIT ---------- */

        init();

        function init() {

            if($scope.model.isContainer) {

                contentTypeResource.getAssignedListViewDataType($scope.model.id)
                    .then(function(dataType) {

                        vm.dataType = dataType;

                        vm.customListViewCreated = checkForCustomListView();

                    });
            }
        }

        /* ----------- LIST VIEW --------- */

        function toggleListView() {

            if($scope.model.isContainer) {

                // add list view data type
                contentTypeResource.getAssignedListViewDataType($scope.model.id)
                    .then(function(dataType) {

                        vm.dataType = dataType;

                        vm.customListViewCreated = checkForCustomListView();

                    });

            } else {

                vm.dataType = {};

            }

        }


        /* ----------- LIST VIEW SETTINGS --------- */

        function toggleEditListViewDataTypeSettings() {

            if(!vm.editDataTypeSettings) {

                // get dataType
                dataTypeResource.getById(vm.dataType.id)
                    .then(function(dataType) {

                        // store data type
                        vm.dataType = dataType;

                        // show edit panel
                        vm.editDataTypeSettings = true;

                    });

            } else {

                // hide edit panel
                vm.editDataTypeSettings = false;

            }

        }

        function saveListViewDataType() {

            var preValues = dataTypeHelper.createPreValueProps(vm.dataType.preValues);

            dataTypeResource.save(vm.dataType, preValues, false).then(function(dataType) {

                // store data type
                vm.dataType = dataType;

                // hide settings panel
                vm.editDataTypeSettings = false;

            });

        }


        /* ---------- CUSTOM LIST VIEW ---------- */

        function createCustomListViewDataType() {

            dataTypeResource.createCustomListView($scope.model.alias).then(function(dataType) {

                // store data type
                vm.dataType = dataType;

                // change state to custom list view
                vm.customListViewCreated = true;

                // show settings panel
                vm.editDataTypeSettings = true;

            });

        }

        function removeCustomListDataType() {

            // delete custom list view data type
            dataTypeResource.deleteById(vm.dataType.id).then(function(dataType) {

                // get default data type
                contentTypeResource.getAssignedListViewDataType($scope.model.id)
                    .then(function(dataType) {

                        // store data type
                        vm.dataType = dataType;

                        // change state to default list view
                        vm.customListViewCreated = false;

                    });

            });

        }


        function checkForCustomListView() {
            return vm.dataType.name === "List View - " + $scope.model.alias;
        }


    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.ListViewController", ListViewController);

})();
