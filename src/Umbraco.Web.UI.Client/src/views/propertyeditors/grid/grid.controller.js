angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.GridController",
    function ($scope, $http, assetsService, $rootScope, dialogService, gridService, mediaResource, imageHelper, $timeout) {

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

        $scope.addTemplate = function (template) {
            $scope.model.value = angular.copy(template);
            
            //default row data
            _.forEach($scope.model.value.sections, function(section){
                $scope.initSection(section);
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
            $scope.initRow(row);
            
            column.rows.push(row);
        };

        $scope.removeRow = function (section, $index) {
            if (section.rows.length > 0) {
                section.rows.splice($index, 1);
                $scope.openRTEToolbarId = null;

                $scope.initContent();
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
            return guid();
        };

        var guid = (function () {
            function s4() {
                return Math.floor((1 + Math.random()) * 0x10000)
                           .toString(16)
                           .substring(1);
            }
            return function () {
                return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
                       s4() + '-' + s4() + s4() + s4();
            };
        })();

        $scope.addControl = function (editor, cell, index){
            var newControl = {
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
        $scope.initContent = function() {
            var clear = true;

            if ($scope.model.value && $scope.model.value.sections && $scope.model.value.sections.length > 0) {
                _.forEach($scope.model.value.sections, function(section){
                    
                    $scope.initSection(section);

                    //we do this to ensure that the grid can be reset by deleting the last row
                    if(section.rows.length > 0){
                        clear = false;
                    }
                });
            }

            if(clear){
                $scope.model.value = undefined;
            }
        };

        $scope.initSection = function(section){
            section.$percentage = $scope.percentage(section.grid);

            var layouts = $scope.model.config.items.layouts;

            if(section.allowed && section.allowed.length > 0){
                section.$allowedLayouts = _.filter(layouts, function(layout){
                    return _.indexOf(section.allowed, layout.name) >= 0;
                });
            }else{
                section.$allowedLayouts = layouts;
            }

            if(!section.rows){
                section.rows = [];
            }else{
                _.forEach(section.rows, function(row){
                    $scope.initRow(row);
                });    
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
                }else{
                    _.forEach(area.controls, function(control, index){
                        $scope.initControl(control, index);
                    });
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
            control.$uniqueId = $scope.setUniqueId();

            if(!control.$editorPath){
                //if its a path
                if(_.indexOf(control.editor.view, "/") >= 0){
                    control.$editorPath = control.editor.view;
                }else{
                    //use convention
                    control.$editorPath = "views/propertyeditors/grid/editors/" + control.editor.view + ".html";
                }
            }
        };
        

        gridService.getGridEditors().then(function(response){
            $scope.availableEditors = response.data;

            $scope.contentReady = true;

            // *********************************************
            // Init grid
            // *********************************************
            $scope.initContent();

        });
    });