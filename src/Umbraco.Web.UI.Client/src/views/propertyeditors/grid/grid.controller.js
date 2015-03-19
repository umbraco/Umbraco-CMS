angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.GridController",
    function ($scope, $http, assetsService, $rootScope, dialogService, gridService, mediaResource, imageHelper, $timeout, umbRequestHelper) {

        // Grid status variables
        $scope.currentRow = null;
        $scope.currentCell = null;
        $scope.currentToolsControl = null;
        $scope.currentControl = null;
        $scope.openRTEToolbarId = null;
        $scope.hasSettings = false;

        // *********************************************
        // Sortable options
        // *********************************************

        $scope.sortableOptions = {
            distance: 10,
            cursor: "move",
            placeholder: 'ui-sortable-placeholder',
            handle: '.cell-tools-move',
            forcePlaceholderSize: true,
            tolerance: "pointer",
            zIndex: 999999999999999999,
            scrollSensitivity: 100,
            cursorAt: {
                top: 45,
                left: 90
            },

            sort: function (event, ui) {
                /* prevent vertical scroll out of the screen */
                var max = $(".usky-grid").width() - 150;
                if (parseInt(ui.helper.css('left')) > max) {
                    ui.helper.css({ 'left': max + "px" })
                }
                if (parseInt(ui.helper.css('left')) < 20) {
                    ui.helper.css({ 'left': 20 })
                }
            },

            start: function (e, ui) {
                ui.item.find('.mceNoEditor').each(function () {
                    tinyMCE.execCommand('mceRemoveEditor', false, $(this).attr('id'));
                });
            },

            stop: function (e, ui) {
                ui.item.parents(".usky-column").find('.mceNoEditor').each(function () {
                    tinyMCE.execCommand('mceRemoveEditor', false, $(this).attr('id'));
                    tinyMCE.execCommand('mceAddEditor', false, $(this).attr('id'));
                });
            }
        };

        var notIncludedRte = [];
        var cancelMove = false;

        $scope.sortableOptionsCell = {
            distance: 10,
            cursor: "move",
            placeholder: "ui-sortable-placeholder",
            handle: '.cell-tools-move',
            connectWith: ".usky-cell",
            forcePlaceholderSize: true,
            tolerance: "pointer",
            zIndex: 999999999999999999,
            scrollSensitivity: 100,
            cursorAt: {
                top: 45,
                left: 90
            },

            sort: function (event, ui) {
                /* prevent vertical scroll out of the screen */
                var position = parseInt(ui.item.parent().offset().left) + parseInt(ui.helper.css('left')) - parseInt($(".usky-grid").offset().left);
                var max = $(".usky-grid").width() - 220;
                if (position > max) {
                    ui.helper.css({ 'left': max - parseInt(ui.item.parent().offset().left) + parseInt($(".usky-grid").offset().left) + "px" })
                }
                if (position < 0) {
                    ui.helper.css({ 'left': 0 - parseInt(ui.item.parent().offset().left) + parseInt($(".usky-grid").offset().left) + "px" })
                }
            },

            over: function (event, ui) {
                allowedEditors = $(event.target).scope().area.allowed;

                if ($.inArray(ui.item.scope().control.editor.alias, allowedEditors) < 0 && allowedEditors) {
                    ui.placeholder.hide();
                    cancelMove = true;
                }
                else {
                    ui.placeholder.show();
                    cancelMove = false;
                }

            },

            update: function (event, ui) {
                if (!ui.sender) {
                    if (cancelMove) {
                        ui.item.sortable.cancel();
                    }
                    ui.item.parents(".usky-cell").find('.mceNoEditor').each(function () {
                        if ($.inArray($(this).attr('id'), notIncludedRte) < 0) {
                            notIncludedRte.splice(0, 0, $(this).attr('id'));
                        }
                    });
                }
                else {
                    $(event.target).find('.mceNoEditor').each(function () {
                        if ($.inArray($(this).attr('id'), notIncludedRte) < 0) {
                            notIncludedRte.splice(0, 0, $(this).attr('id'));
                        }
                    });
                }

            },

            start: function (e, ui) {
                ui.item.find('.mceNoEditor').each(function () {
                    notIncludedRte = [];
                    tinyMCE.execCommand('mceRemoveEditor', false, $(this).attr('id'));
                });
            },

            stop: function (e, ui) {
                ui.item.parents(".usky-cell").find('.mceNoEditor').each(function () {
                    if ($.inArray($(this).attr('id'), notIncludedRte) < 0) {
                        notIncludedRte.splice(0, 0, $(this).attr('id'));
                    }
                });
                $timeout(function () {
                    _.forEach(notIncludedRte, function (id) {
                        tinyMCE.execCommand('mceRemoveEditor', false, id);
                        tinyMCE.execCommand('mceAddEditor', false, id);
                        console.info("stop " + id);
                    });
                }, 500, false);
            }

        };

        // *********************************************
        // Add items overlay menu
        // *********************************************
        $scope.overlayMenu = {
            show: false,
            style: {},
            area: undefined,
            key: undefined
        };

        $scope.addItemOverlay = function (event, area, index, key) {
            $scope.overlayMenu.area = area;
            $scope.overlayMenu.index = index;
            $scope.overlayMenu.style = {};
            $scope.overlayMenu.key = key;

            //todo calculate position...
            var offset = $(event.target).offset();
            var height = $(window).height();
            var width = $(window).width();

            if ((height - offset.top) < 250) {
                $scope.overlayMenu.style.bottom = 0;
                $scope.overlayMenu.style.top = "initial";
            } else if (offset.top < 300) {
                $scope.overlayMenu.style.top = 190;
            }

            $scope.overlayMenu.show = true;
        };

        $scope.closeItemOverlay = function () {
            $scope.currentControl = null;
            $scope.overlayMenu.show = false;
            $scope.overlayMenu.key = undefined;
        };

        // *********************************************
        // Template management functions
        // *********************************************

        $scope.addTemplate = function (template) {
            $scope.model.value = angular.copy(template);

            //default row data
            _.forEach($scope.model.value.sections, function (section) {
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

        $scope.setWarnighlightRow = function (row) {
            $scope.currentWarnhighlightRow = row;
        };

        $scope.disableWarnhighlightRow = function () {
            $scope.currentWarnhighlightRow = null;
        };

        $scope.setInfohighlightRow = function (row) {
            $scope.currentInfohighlightRow = row;
        };

        $scope.disableInfohighlightRow = function () {
            $scope.currentInfohighlightRow = null;
        };

        $scope.getAllowedLayouts = function (column) {
            var layouts = $scope.model.config.items.layouts;

            if (column.allowed && column.allowed.length > 0) {
                return _.filter(layouts, function (layout) {
                    return _.indexOf(column.allowed, layout.name) >= 0;
                });
            } else {
                return layouts;
            }
        };

        $scope.addRow = function (section, layout) {

            //copy the selected layout into the rows collection
            var row = angular.copy(layout);

            // Init row value
            row = $scope.initRow(row);

            // Push the new row
            if (row) {
                section.rows.push(row);
            }
        };

        $scope.removeRow = function (section, $index) {
            if (section.rows.length > 0) {
                section.rows.splice($index, 1);
                $scope.currentRow = null;
                $scope.openRTEToolbarId = null;
                $scope.initContent();
            }
        };

        $scope.editGridItemSettings = function (gridItem, itemType) {

            dialogService.open(
                {
                    template: "views/propertyeditors/grid/dialogs/config.html",
                    gridItem: gridItem,
                    config: $scope.model.config,
                    itemType: itemType,
                    callback: function (data) {

                        gridItem.styles = data.styles;
                        gridItem.config = data.config;

                    }
                });

        };

        // *********************************************
        // Area management functions
        // *********************************************

        $scope.setCurrentCell = function (cell) {
            $scope.currentCell = cell;
        };

        $scope.disableCurrentCell = function () {
            $scope.currentCell = null;
        };

        $scope.cellPreview = function (cell) {
            if (cell && cell.$allowedEditors) {
                var editor = cell.$allowedEditors[0];
                return editor.icon;
            } else {
                return "icon-layout";
            }
        };

        $scope.setInfohighlightArea = function (cell) {
            $scope.currentInfohighlightArea = cell;
        };

        $scope.disableInfohighlightArea = function () {
            $scope.currentInfohighlightArea = null;
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

        $scope.setWarnhighlightControl = function (Control) {
            $scope.currentWarnhighlightControl = Control;
        };

        $scope.disableWarnhighlightControl = function (Control) {
            $scope.currentWarnhighlightControl = null;
        };

        $scope.setInfohighlightControl = function (Control) {
            $scope.currentInfohighlightControl = Control;
        };

        $scope.disableInfohighlightControl = function (Control) {
            $scope.currentInfohighlightControl = null;
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

        $scope.addControl = function (editor, cell, index) {
            $scope.closeItemOverlay();

            var newControl = {
                value: null,
                editor: editor,
                $initializing: true
            };

            if (index === undefined) {
                index = cell.controls.length;
            }

            //populate control
            $scope.initControl(newControl, index + 1);

            cell.controls.splice(index + 1, 0, newControl);
        };

        $scope.addTinyMce = function (cell) {
            var rte = $scope.getEditor("rte");
            $scope.addControl(rte, cell);
        };

        $scope.getEditor = function (alias) {
            return _.find($scope.availableEditors, function (editor) { return editor.alias === alias; });
        };

        $scope.removeControl = function (cell, $index) {
            $scope.currentControl = null;
            cell.controls.splice($index, 1);
        };

        $scope.percentage = function (spans) {
            return ((spans / $scope.model.config.items.columns) * 100).toFixed(1);
        };


        $scope.clearPrompt = function (scopedObject, e) {
            scopedObject.deletePrompt = false;
            e.preventDefault();
            e.stopPropagation();
        }

        $scope.showPrompt = function (scopedObject) {
            scopedObject.deletePrompt = true;
        }


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
        $scope.initContent = function () {
            var clear = true;

            //settings indicator shortcut
            if ($scope.model.config.items.config || $scope.model.config.items.styles) {
                $scope.hasSettings = true;
            }

            //ensure the grid has a column value set, if nothing is found, set it to 12
            if ($scope.model.config.items.columns && angular.isString($scope.model.config.items.columns)) {
                $scope.model.config.items.columns = parseInt($scope.model.config.items.columns);
            } else {
                $scope.model.config.items.columns = 12;
            }

            if ($scope.model.value && $scope.model.value.sections && $scope.model.value.sections.length > 0) {
                _.forEach($scope.model.value.sections, function (section, index) {

                    if (section.grid > 0) {
                        $scope.initSection(section);

                        //we do this to ensure that the grid can be reset by deleting the last row
                        if (section.rows.length > 0) {
                            clear = false;
                        }
                    } else {
                        $scope.model.value.sections.splice(index, 1);
                    }
                });
            } else if ($scope.model.config.items.templates && $scope.model.config.items.templates.length === 1) {
                $scope.addTemplate($scope.model.config.items.templates[0]);
            }

            if (clear) {
                $scope.model.value = undefined;
            }
        };

        $scope.initSection = function (section) {
            section.$percentage = $scope.percentage(section.grid);

            var layouts = $scope.model.config.items.layouts;

            if (section.allowed && section.allowed.length > 0) {
                section.$allowedLayouts = _.filter(layouts, function (layout) {
                    return _.indexOf(section.allowed, layout.name) >= 0;
                });
            } else {
                section.$allowedLayouts = layouts;
            }

            if (!section.rows) {
                section.rows = [];
            } else {
                _.forEach(section.rows, function (row, index) {
                    if (!row.$initialized) {
                        var initd = $scope.initRow(row);

                        //if init fails, remove
                        if (!initd) {
                            section.rows.splice(index, 1);
                        } else {
                            section.rows[index] = initd;
                        }
                    }
                });
            }
        };


        // *********************************************
        // Init layout / row
        // *********************************************
        $scope.initRow = function (row) {

            //merge the layout data with the original config data
            //if there are no config info on this, splice it out
            var original = _.find($scope.model.config.items.layouts, function (o) { return o.name === row.name; });

            if (!original) {
                return null;
            } else {
                //make a copy to not touch the original config
                original = angular.copy(original);
                original.styles = row.styles;
                original.config = row.config;

                //sync area configuration
                _.each(original.areas, function (area, areaIndex) {


                    if (area.grid > 0) {
                        var currentArea = row.areas[areaIndex];

                        if (currentArea) {
                            area.config = currentArea.config;
                            area.styles = currentArea.styles;
                        }

                        //copy over existing controls into the new areas
                        if (row.areas.length > areaIndex && row.areas[areaIndex].controls) {
                            area.controls = currentArea.controls;

                            _.forEach(area.controls, function (control, controlIndex) {
                                $scope.initControl(control, controlIndex);
                            });

                        } else {
                            area.controls = [];
                        }

                        //set width
                        area.$percentage = $scope.percentage(area.grid);
                        area.$uniqueId = $scope.setUniqueId();

                        //set editor permissions
                        if (!area.allowed || area.allowAll === true) {
                            area.$allowedEditors = $scope.availableEditors;
                            area.$allowsRTE = true;
                        } else {
                            area.$allowedEditors = _.filter($scope.availableEditors, function (editor) {
                                return _.indexOf(area.allowed, editor.alias) >= 0;
                            });

                            if (_.indexOf(area.allowed, "rte") >= 0) {
                                area.$allowsRTE = true;
                            }
                        }
                    } else {
                        original.areas.splice(areaIndex, 1);
                    }
                });

                //replace the old row
                original.$initialized = true;

                //set a disposable unique ID
                original.$uniqueId = $scope.setUniqueId();

                //set a no disposable unique ID (util for row styling)
                original.id = !row.id ? $scope.setUniqueId() : row.id;

                return original;
            }

        };


        // *********************************************
        // Init control
        // *********************************************

        $scope.initControl = function (control, index) {
            control.$index = index;
            control.$uniqueId = $scope.setUniqueId();

            //error handling in case of missing editor..
            //should only happen if stripped earlier 
            if (!control.editor) {
                control.$editorPath = "views/propertyeditors/grid/editors/error.html";
            }

            if (!control.$editorPath) {
                var editorConfig = $scope.getEditor(control.editor.alias);

                if (editorConfig) {
                    control.editor = editorConfig;

                    //if its an absolute path
                    if (control.editor.view.startsWith("/") || control.editor.view.startsWith("~/")) {
                        control.$editorPath = umbRequestHelper.convertVirtualToAbsolutePath(control.editor.view);
                    }
                    else {
                        //use convention
                        control.$editorPath = "views/propertyeditors/grid/editors/" + control.editor.view + ".html";
                    }
                }
                else {
                    control.$editorPath = "views/propertyeditors/grid/editors/error.html";
                }
            }


        };


        gridService.getGridEditors().then(function (response) {
            $scope.availableEditors = response.data;

            $scope.contentReady = true;

            // *********************************************
            // Init grid
            // *********************************************
            $scope.initContent();

        });
    });
