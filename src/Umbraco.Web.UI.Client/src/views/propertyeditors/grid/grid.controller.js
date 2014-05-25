angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.GridController",
    function ($scope, $http, assetsService, $rootScope, dialogService, gridService, mediaResource, imageHelper, $timeout) {

        var gridConfigPath = $scope.model.config.items.gridConfigPath;

        if(!gridConfigPath){
            gridConfigPath = "views/propertyeditors/grid/grid.default.config.js";
        }

        assetsService.loadJs(gridConfigPath).then(function(){

                // Grid config
                $scope.uSkyGridConfig = uSkyGridConfig;

                // Grid status variables 
                $scope.currentRow = null;
                $scope.currentCell = null;
                $scope.currentToolsControl = null;
                $scope.currentControl = null;
                $scope.openRTEToolbarId = null;

                gridService.getGridEditors().then(function(response){
                    $scope.availableEditors = response.data;
                });
                
                // *********************************************
                // Sortable options
                // *********************************************

                $scope.sortableOptions = {
                    distance: 10,
                    cursor: "move",
                    placeholder: 'ui-sortable-placeholder',
                    handle: '.cell-tools-move',
                    start: function (e, ui) {
                        ui.item.find('.mceNoEditor').each(function () {
                            tinyMCE.execCommand('mceRemoveEditor', false, $(this).attr('id'));

                        });
                    },
                    stop: function (e, ui) {
                        ui.item.find('.mceNoEditor').each(function () {
                            tinyMCE.execCommand('mceAddEditor', false, $(this).attr('id'));
                        });
                    }
                };

                // *********************************************
                // Template management functions
                // *********************************************

                $scope.checkContent = function() {
                    var isEmpty = true;
                    if ($scope.model.value && 
                        $scope.model.value.columns) {
                        angular.forEach($scope.model.value.columns, function (value, key) {
                            if ( value.rows && value.rows.length > 0) {
                                isEmpty = false;
                            }
                        });
                    }

                    if (isEmpty)
                    {
                        $scope.model.value = undefined;
                    }

                }

                $scope.addTemplate = function (template) {
                    $scope.model.value = {
                        gridWidth: "",
                        columns: []
                    };

                    angular.forEach(template.columns, function (value, key) {
                        var newCol = angular.copy(value);
                        newCol.rows = [];
                        $scope.model.value.columns.splice($scope.model.value.columns.length + 1, 0, newCol);
                    });
                }


                // *********************************************
                // Row management function
                // *********************************************

                $scope.setCurrentRow = function (row) {
                    $scope.currentRow = row;
                }

                $scope.disableCurrentRow = function () {
                    $scope.currentRow = null;
                }

                
                $scope.addRow = function (column, cellModel) {

                    column.rows.splice(column.rows.length + 1, 0,
                    {
                        uniqueId: $scope.setUniqueId(),
                        cells: [],
                        cssClass: ''
                    });

                    for (var i = 0; i < cellModel.models.length; i++) {

                        var cells = column.rows[column.rows.length - 1].cells;
                        var model = angular.copy(cellModel.models[i]);

                        cells.splice(
                            cells.length + 1, 0,
                            {
                                model: model,
                                controls: []
                            });
                    }
                };

                $scope.removeRow = function (column, $index) {
                    if (column.rows.length > 0) {
                        column.rows.splice($index, 1);
                        $scope.openRTEToolbarId = null;
                        $scope.checkContent();
                    }
                }

                // *********************************************
                // Cell management functions
                // *********************************************

                $scope.setCurrentCell = function (cell) {
                    $scope.currentCell = cell;
                }

                $scope.disableCurrentCell = function (cell) {
                    $scope.currentCell = null;
                }

                $scope.cellPreview = function(cell){
                    if($scope.availableEditors && cell && cell.allowed && angular.isArray(cell.allowed)){
                        var editor = $scope.getEditor(cell.allowed[0]);
                        return editor.icon;
                    }else{
                        return "icon-layout";
                    }
                }

                // *********************************************
                // Control management functions
                // *********************************************
                $scope.setCurrentControl = function (Control) {
                    $scope.currentControl = Control;
                };

                $scope.disableCurrentControl = function (Control) {
                    $scope.currentControl = null;
                };

                $scope.setCurrentToolsControl = function (Control) {
                    $scope.currentToolsControl = Control;
                };

                $scope.disableCurrentToolsControl = function (Control) {
                    $scope.currentToolsControl = null;
                };

                $scope.setCurrentRemoveControl = function (Control) {
                    $scope.currentRemoveControl = Control;
                };

                $scope.disableCurrentRemoveControl = function (Control) {
                    $scope.currentRemoveControl = null;
                };

                $scope.setCurrentMoveControl = function (Control) {
                    $scope.currentMoveControl = Control;
                };

                $scope.disableCurrentMoveControl = function (Control) {
                    $scope.currentMoveControl = null;
                };

                $scope.setUniqueId = function (cell, index) {
                    var date = new Date();
                    var components = [
                        date.getYear(),
                        date.getMonth(),
                        date.getDate(),
                        date.getHours(),
                        date.getMinutes(),
                        date.getSeconds(),
                        date.getMilliseconds()
                    ];
                    return components.join("");
                };

                $scope.setEditorPath = function(control){
                    control.editorPath = "views/propertyeditors/grid/editors/" + control.editor.view + ".html";
                };

                $scope.addControl = function (editor, cell, index){
                    var newId = $scope.setUniqueId();
                    var newControl = {
                        uniqueId: newId,
                        value: null,
                        editor: editor
                    };

                    if (index === undefined) {
                        index = cell.controls.length;
                    }

                    cell.controls.splice(index + 1, 0, newControl);
                };

                $scope.addTinyMce = function(cell){
                    var rte = _.find($scope.availableEditors, function(editor){return editor.alias === "rte";});
                    $scope.addControl(rte, cell);
                };

                $scope.getEditor = function(alias){
                    return  _.find($scope.availableEditors, function(editor){return editor.alias === alias});
                };

                $scope.removeControl = function (cell, $index) {
                    cell.controls.splice($index, 1);
                };

                $scope.allowedControl = function (editor, cell){
                    if(cell.model.allowed && angular.isArray(cell.model.allowed)){
                        return _.contains(cell.model.allowed, editor.alias);
                    }else{
                        return true;
                    }
                };


                // *********************************************
                // Init grid
                // *********************************************

                /* init grid data */
                if ($scope.model.value && $scope.model.value != "") {
                    if (!$scope.model.config.items.enableGridWidth) {
                        $scope.model.value.gridWidth = $scope.model.config.items.defaultGridWith;
                    }
                }

                $scope.checkContent();

            });

        // *********************************************
        // asset styles
        // *********************************************

        //assetsService.loadCss("/App_Plugins/Lecoati.uSky.Grid/lib/jquery-ui-1.10.4.custom/css/ui-lightness/jquery-ui-1.10.4.custom.min.css");
        assetsService.loadCss($scope.model.config.items.approvedBackgroundCss);

    });