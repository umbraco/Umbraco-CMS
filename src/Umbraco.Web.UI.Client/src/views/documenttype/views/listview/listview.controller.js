/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.ListViewController
 * @function
 *
 * @description
 * The controller for the content type editor list view section
 */
function ListViewController($scope, contentTypeResource, dataTypeResource, dataTypeHelper) {

    /* ---------- SCOPE VARIABLES ---------- */

    $scope.listView = {};
    $scope.listView.dataType = {};
    $scope.listView.editDataTypeSettings = false;
    $scope.listView.customListViewCreated = false;


    /* ---------- INIT ---------- */

    init();

    function init() {

        if($scope.contentType.isContainer) {

            contentTypeResource.getAssignedListViewDataType($scope.contentType.id)
                .then(function(dataType) {

                    $scope.listView.dataType = dataType;

                    $scope.listView.customListViewCreated = checkForCustomListView();

                });
        }
    }

    /* ----------- LIST VIEW --------- */

    $scope.toggleListView = function() {

        if($scope.contentType.isContainer) {

            // add list view data type
            contentTypeResource.getAssignedListViewDataType($scope.contentType.id)
                .then(function(dataType) {

                    $scope.listView.dataType = dataType;

                    $scope.listView.customListViewCreated = checkForCustomListView();

                });

        } else {

            $scope.listView.dataType = {};

        }

    };


    /* ----------- LIST VIEW SETTINGS --------- */

    $scope.toggleEditListViewDataTypeSettings = function() {

        if(!$scope.listView.editDataTypeSettings) {

            // get dataType
            dataTypeResource.getById($scope.listView.dataType.id)
                .then(function(dataType) {

                    // store data type
                    $scope.listView.dataType = dataType;

                    // show edit panel
                    $scope.listView.editDataTypeSettings = true;

                });

        } else {

            // hide edit panel
            $scope.listView.editDataTypeSettings = false;

        }

    };

    $scope.saveListViewDataType = function() {

        var preValues = dataTypeHelper.createPreValueProps($scope.listView.dataType.preValues);

        dataTypeResource.save($scope.listView.dataType, preValues, false).then(function(dataType) {

            // store data type
            $scope.listView.dataType = dataType;

            // hide settings panel
            $scope.listView.editDataTypeSettings = false;

        });

    };


    /* ---------- CUSTOM LIST VIEW ---------- */

    $scope.createCustomListViewDataType = function() {

        dataTypeResource.createCustomListView($scope.contentType.alias).then(function(dataType) {

            // store data type
            $scope.listView.dataType = dataType;

            // change state to custom list view
            $scope.listView.customListViewCreated = true;

            // show settings panel
            $scope.listView.editDataTypeSettings = true;

        });

    };

    $scope.removeCustomListDataType = function() {

        // delete custom list view data type
        dataTypeResource.deleteById($scope.listView.dataType.id).then(function(dataType) {

            // get default data type
            contentTypeResource.getAssignedListViewDataType($scope.contentType.id)
                .then(function(dataType) {

                    // store data type
                    $scope.listView.dataType = dataType;

                    // change state to default list view
                    $scope.listView.customListViewCreated = false;

                });

        });

    };


    function checkForCustomListView() {
        return $scope.listView.dataType.name === "List View - " + $scope.contentType.alias;
    }


}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.ListViewController", ListViewController);
