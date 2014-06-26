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
                        $scope.model.value.sections) {
                        angular.forEach($scope.model.value.sections, function (value, key) {
                            if ( value.rows && value.rows.length > 0) {
                                isEmpty = false;
                            }
                        });
                    }

                    if (isEmpty)
                    {
                        $scope.model.value = undefined;
                    }
                };

                $scope.addTemplate = function (template) {
                    $scope.model.value = angular.copy(template);
                    
                    //default row data
                    _.forEach($scope.model.value.sections, function(section){
                        section.rows = [];
                    });
                };


                // *********************************************
                // Row management function
                // *********************************************

                $scope.setCurrentRow = function (row) {
                    $scope.currentRow = row;
                };

                $scope.disableCurrentRow = function () {
                    $scope.currentRow = null;
                };

                
                $scope.getAllowedLayouts = function(column){
                    var layouts = $scope.model.config.items.layouts;

                    if(column.allowed && column.allowed.length > 0){
                        return _.filter(layouts, function(layout){
                            return _.indexOf(column.allowed, layout.name) >= 0;
                        });
                    }else{
                        return layouts;
                    } 
                };

                $scope.addRow = function (column, layout) {
                    //copy the selected layout into the rows collection
                    var row = angular.copy(layout);
                    column.rows.push(row);
                };

                $scope.removeRow = function (column, $index) {
                    if (column.rows.length > 0) {
                        column.rows.splice($index, 1);
                        $scope.openRTEToolbarId = null;
                        $scope.checkContent();
                    }
                };


                // *********************************************
                // Cell management functions
                // *********************************************

                $scope.setCurrentCell = function (cell) {
                    $scope.currentCell = cell;
                };

                $scope.disableCurrentCell = function (cell) {
                    $scope.currentCell = null;
                };

                $scope.cellPreview = function(cell){
                    if(cell && cell.$allowedEditors){
                        var editor = cell.$allowedEditors[0];
                        return editor.icon;
                    }else{
                        return "icon-layout";
                    }
                };




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


                $scope.addControl = function (editor, cell, index){
                    var newId = $scope.setUniqueId();
                    var newControl = {
                        $uniqueId: newId,
                        value: null,
                        editor: editor
                    };

                    if (index === undefined) {
                        index = cell.controls.length;
                    }

                    //populate control
                    $scope.initControl(newControl, index+1);

                    cell.controls.splice(index + 1, 0, newControl);
                };

                $scope.addTinyMce = function(cell){
                    var rte = $scope.getEditor("rte");
                    $scope.addControl(rte, cell);
                };

                $scope.getEditor = function(alias){
                    return  _.find($scope.availableEditors, function(editor){return editor.alias === alias});
                };

                $scope.removeControl = function (cell, $index) {
                    cell.controls.splice($index, 1);
                };

                $scope.percentage = function(spans){
                    return ((spans/12)*100).toFixed(1);
                };




                // *********************************************
                // Init grid
                // *********************************************
                $scope.checkContent();



                // *********************************************
                // INITIALISATION
                // these methods are called from ng-init on the template
                // so we can controll their first load data
                // 
                // intialisation sets non-saved data like percentage sizing, allowed editors and
                // other data that should all be pre-fixed with $ to strip it out on save
                // *********************************************                

                // *********************************************
                // Init template + sections
                // *********************************************                
                $scope.initSection = function(section){
                    section.$percentage = $scope.percentage(section.grid);

                    var layouts = $scope.model.config.items.layouts;

                    if(section.allowed && sectionn.allowed.length > 0){
                        return _.filter(layouts, function(layout){
                            section.$allowedLayouts = _.indexOf(section.allowed, layout.name) >= 0;
                        });
                    }else{
                        section.$allowedLayouts = layouts;
                    } 
                };


                // *********************************************
                // Init layout / row
                // *********************************************                
                $scope.initRow = function(row){
                    
                    if(!row.areas){
                        row.areas = [];
                    }

                    //set a disposable unique ID
                    row.$uniqueId = $scope.setUniqueId();
                    
                    //populate with data
                    _.forEach(row.areas, function(area){
                        if(!area.controls){
                            area.controls = [];
                        }
                        
                        area.$percentage = $scope.percentage(area.grid);

                        if(!area.allowed){
                            area.$allowedEditors = $scope.availableEditors;
                            area.$allowsRTE = true;
                        }else{
                            area.$allowedEditors = _.filter($scope.availableEditors, function(editor){
                                return _.indexOf(area.allowed, editor.alias) >= 0;
                            });

                            if(_.indexOf(area.allowed,"rte")>=0){
                                area.$allowsRTE = true;
                            }
                        }
                    });
                };



                // *********************************************
                // Init control
                // *********************************************                

                $scope.initControl = function(control, index){
                    control.$index = index;

                    //if its a path
                    if(_.indexOf(control.editor.view, "/") >= 0){
                        control.$editorPath = control.editor.view;
                    }else{
                        //use convention
                        control.$editorPath = "views/propertyeditors/grid/editors/" + control.editor.view + ".html";
                    }
                };
            });

        // *********************************************
        // asset styles
        // *********************************************

        //assetsService.loadCss("/App_Plugins/Lecoati.uSky.Grid/lib/jquery-ui-1.10.4.custom/css/ui-lightness/jquery-ui-1.10.4.custom.min.css");
        //assetsService.loadCss($scope.model.config.items.approvedBackgroundCss);

    });