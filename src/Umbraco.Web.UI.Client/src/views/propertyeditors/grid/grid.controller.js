angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.GridController",
    function ($scope, $http, assetsService, $rootScope, dialogService, mediaResource, imageHelper, $timeout) {

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

                // *********************************************
                // Sortable options
                // *********************************************

                $scope.sortableOptions = {
                    distance: 10,
                    cursor: "move",
                    placeholder: 'ui-sortable-placeholder',
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
                    }

                    angular.forEach(template.columns, function (value, key) {
                        $scope.model.value.columns.splice($scope.model.value.columns.length + 1, 0, {
                            id: value.name,
                            grid: value.grid,
                            percentage: value.percentage,
                            cellModels: value.cellModels,
                            rows: []
                        });
                    });

                }

                // *********************************************
                // RTE toolbar management functions
                // *********************************************

                $scope.openRTEToolbar = function (control) {
                    $scope.openRTEToolbarId = control.tinyMCE.uniqueId;
                }

                $scope.closeRTEToolbar = function (control) {
                    $scope.openRTEToolbarId = null;
                }

                $scope.isOpenRTEToolbar = function (control) {
                    if (control != undefined && control.tinyMCE != undefined) {
                        if ($scope.openRTEToolbarId == control.tinyMCE.uniqueId) {
                            return true;
                        }
                        else {
                            return false;
                        }
                    }
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

                $scope.setBackGroundRow = function (row) {
                    var dialog = dialogService.open({
                        template: '/views/common/dialogs//approvedcolorpicker.html',
                        show: true,
                        dialogData: {
                            cssPath: $scope.model.config.items.approvedBackgroundCss,
                            cssClass: row.cssClass
                        },
                        callback: function (data) {
                            row.cssClass = data;
                        }
                    });
                }

                $scope.boxed = function (cell) {
                    if (cell.boxed == true) {
                        cell.boxed = false;
                    }
                    else {
                        cell.boxed = true;
                    }
                }

                $scope.skipTopMargin = function (row) {
                    if (row.skipTopMargin == true) {
                        row.skipTopMargin = false;
                    }
                    else {
                        row.skipTopMargin = true;
                    }
                }

                $scope.skipBottomMargin = function (row) {
                    if (row.skipBottomMargin == true) {
                        row.skipBottomMargin = false;
                    }
                    else {
                        row.skipBottomMargin = true;
                    }
                }

                $scope.skipLeftMargin = function (row) {
                    if (row.skipLeftMargin == true) {
                        row.skipLeftMargin = false;
                    }
                    else {
                        row.skipLeftMargin = true;
                    }
                }

                $scope.skipRightMargin = function (row) {
                    if (row.skipRightMargin == true) {
                        row.skipRightMargin = false;
                    }
                    else {
                        row.skipRightMargin = true;
                    }
                }

                $scope.skipControlMargin = function (row) {
                    if (row.skipControlMargin == true) {
                        row.skipControlMargin = false;
                    }
                    else {
                        row.skipControlMargin = true;
                    }
                }

                $scope.fullScreen = function (row) {
                    if (row.fullScreen == true) {
                        row.fullScreen = false;
                    }
                    else {
                        row.fullScreen = true;
                    }
                }

                $scope.addRow = function (column, cellModel) {

                    column.rows.splice(column.rows.length + 1, 0,
                    {
                        cells: [],
                        cssClass: '',
                        boxed: $scope.model.config.items.defaultBoxed,
                        skipTopMargin: $scope.model.config.items.defaultSkipTopMargin,
                        skipBottomMargin: $scope.model.config.items.defaultSkipBottomMargin,
                        skipLeftMargin: $scope.model.config.items.defaultSkipLeftMargin,
                        skipRightMargin: $scope.model.config.items.defaultSkipRightMargin,
                        skipControlMargin: $scope.model.config.items.defaultSkipControlMargin,
                        fullScreen: $scope.model.config.items.defaultFullScreen
                    });

                    for (var i = 0; i < cellModel.models.length; i++) {

                        var cells = column.rows[column.rows.length - 1].cells

                        cells.splice(
                            cells.length + 1, 0,
                            {
                                model: {
                                    grid: cellModel.models[i].grid,
                                    percentage: cellModel.models[i].percentage,
                                },
                                boxed: false,
                                controls: []
                            });
                    }
                }

                $scope.removeRow = function (column, $index) {
                    if (column.rows.length > 0) {
                        column.rows.splice($index, 1);
                        $scope.openRTEToolbarId = null
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

                // *********************************************
                // Control management functions
                // *********************************************

                $scope.setCurrentControl = function (Control) {
                    $scope.currentControl = Control;
                }

                $scope.disableCurrentControl = function (Control) {
                    $scope.currentControl = null;
                }

                $scope.setCurrentToolsControl = function (Control) {
                    $scope.currentToolsControl = Control;
                }

                $scope.disableCurrentToolsControl = function (Control) {
                    $scope.currentToolsControl = null;
                }

                $scope.setCurrentRemoveControl = function (Control) {
                    $scope.currentRemoveControl = Control;
                }

                $scope.disableCurrentRemoveControl = function (Control) {
                    $scope.currentRemoveControl = null;
                }


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

                }

                $scope.addTinyMce = function (cell, index) {

                    var newId = $scope.setUniqueId();
                    var newControl = {
                        uniqueId: newId,
                        tinyMCE: {
                            uniqueId: newId,
                            value: "",
                            label: 'cellText',
                            description: 'Load some stuff here',
                            view: 'rte',
                            config: {
                                editor: {
                                    toolbar: [], //TODO
                                    stylesheets: [], //TODO
                                    dimensions: { height: 0, width: 0 } //TODO
                                }
                            }
                        }
                    }

                    if (index == undefined) {
                        index = cell.controls.length
                    }

                    cell.controls.splice(index + 1, 0, newControl);

                    $scope.openRTEToolbar(newControl);
                    
                    $timeout(function(){
                        tinymce.get(newId).focus();
                    }, 500);
                }

                $scope.addMedia = function (cell, index) {

                    dialogService.mediaPicker({
                        multiPicker: false,
                        callback: function (data) {

                            if (!false) {
                                data = [data];
                            }

                            _.each(data, function (media, i) {
                                media.src = imageHelper.getImagePropertyValue({ imageModel: media });
                                media.thumbnail = imageHelper.getThumbnailFromPath(media.src);
                                var newControl = {
                                    uniqueId: $scope.setUniqueId(),
                                    media: {
                                        id: media.id,
                                        src: media.src,
                                        thumbnail: media.thumbnail
                                    }
                                }
                                if (index == undefined) {
                                    index = cell.controls.length
                                }

                                cell.controls.splice(index + 1, 0, newControl);
                            });
                        }
                    });

                }

                $scope.addEmbed = function (cell, index) {
                    dialogService.embedDialog({
                        callback: function (data) {
                            var newControl = {
                                uniqueId: $scope.setUniqueId(),
                                embed: {
                                    content: data
                                }
                            }
                            if (index == undefined) {
                                index = cell.controls.length
                            }

                            cell.controls.splice(index + 1, 0, newControl);
                        }
                    });

                }

                $scope.addMacro = function (cell, index) {
                    var dialogData = {};
                    dialogService.macroPicker({
                        dialogData: dialogData,
                        callback: function (data) {
                            var newControl = {
                                uniqueId: $scope.setUniqueId(),
                                macro: {
                                    syntax: data.syntax,
                                    macroAlias: data.macroAlias,
                                    marcoParamsDictionary: data.macroParamsDictionary
                                }
                            }
                            if (index == undefined) {
                                index = cell.controls.length
                            }

                            cell.controls.splice(index + 1, 0, newControl);
                        }
                    });

                }

                $scope.removeControl = function (cell, $index) {
                    cell.controls.splice($index, 1);
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