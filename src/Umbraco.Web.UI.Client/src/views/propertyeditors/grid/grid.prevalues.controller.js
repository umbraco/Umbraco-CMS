angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.GridPrevalueEditorController",
    function ($scope, $http, assetsService, $rootScope, dialogService, mediaResource, imageHelper, $timeout) {

        var emptyModel = { 
            enableGridWidth: true,
            defaultGridWith: "",
            enableBackground: true,
            enableSkipTopMargin: true,
            defaultSkipTopMargin: false,
            enableSkipBottomMargin: true,
            defaultSkipBottomMargin: false,
            enableSkipLeftMargin: true,
            defaultSkipLeftMargin: false,
            enableSkipRightMargin: true,
            defaultSkipRightMargin: false,
            enableSkipControlMargin: true,
            defaultSkipControlMargin: false,
            enableFullScreen: true,
            defaultFullScreen: false,
            enableBoxed: true,
            defaultBoxed: false,
            enableRte: true,
            enableMedia: true,
            enableMacro: true,
            enableMultiCells: true,
            approvedBackgroundCss: "views/propertyeditors/grid/config/grid.default.backgrounds.css",
            gridConfigPath: "views/propertyeditors/grid/config/grid.default.config.js"
        }

        /* init grid data */
        if (!$scope.model.value || $scope.model.value == "") {
            $scope.model.value = emptyModel;
        }

    })