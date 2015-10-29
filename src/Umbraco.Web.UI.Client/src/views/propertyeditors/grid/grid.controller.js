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

        var draggedRteSettings;

        $scope.sortableOptions = {
            distance: 10,
            cursor: "move",
            placeholder: "ui-sortable-placeholder",
            handle: ".cell-tools-move",
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
                if (parseInt(ui.helper.css("left")) > max) {
                    ui.helper.css({ "left": max + "px" });
                }
                if (parseInt(ui.helper.css("left")) < 20) {
                    ui.helper.css({ "left": 20 });
                }
            },

            start: function (e, ui) {
                draggedRteSettings = {};
                ui.item.find(".mceNoEditor").each(function () {
                    // remove all RTEs in the dragged row and save their settings
                    var id = $(this).attr("id");
                    draggedRteSettings[id] = _.findWhere(tinyMCE.editors, { id: id }).settings;
                    tinyMCE.execCommand("mceRemoveEditor", false, id);
                });
            },

            stop: function (e, ui) {
                // reset all RTEs affected by the dragging
                ui.item.parents(".usky-column").find(".mceNoEditor").each(function () {
                    var id = $(this).attr("id");
                    draggedRteSettings[id] = draggedRteSettings[id] || _.findWhere(tinyMCE.editors, { id: id }).settings;
                    tinyMCE.execCommand("mceRemoveEditor", false, id);
                    tinyMCE.init(draggedRteSettings[id]);
                });
            }
        };

        var notIncludedRte = [];
        var cancelMove = false;

        $scope.sortableOptionsCell = {
            distance: 10,
            cursor: "move",
            placeholder: "ui-sortable-placeholder",
            handle: ".cell-tools-move",
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
                var position = parseInt(ui.item.parent().offset().left) + parseInt(ui.helper.css("left")) - parseInt($(".usky-grid").offset().left);
                var max = $(".usky-grid").width() - 220;
                if (position > max) {
                    ui.helper.css({ "left": max - parseInt(ui.item.parent().offset().left) + parseInt($(".usky-grid").offset().left) + "px" });
                }
                if (position < 0) {
                    ui.helper.css({ "left": 0 - parseInt(ui.item.parent().offset().left) + parseInt($(".usky-grid").offset().left) + "px" });
                }
            },

            over: function (event, ui) {
                var allowedEditors = $(event.target).scope().area.allowed;

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
                // add all RTEs which are affected by the dragging
                if (!ui.sender) {
                    if (cancelMove) {
                        ui.item.sortable.cancel();
                    }
                    ui.item.parents(".usky-cell").find(".mceNoEditor").each(function () {
                        if ($.inArray($(this).attr("id"), notIncludedRte) < 0) {
                            notIncludedRte.splice(0, 0, $(this).attr("id"));
                        }
                    });
                }
                else {
                    $(event.target).find(".mceNoEditor").each(function () {
                        if ($.inArray($(this).attr("id"), notIncludedRte) < 0) {
                            notIncludedRte.splice(0, 0, $(this).attr("id"));
                        }
                    });
                }

            },

            start: function (e, ui) {
                // reset dragged RTE settings in case a RTE isn't dragged
                draggedRteSettings = undefined;

                ui.item.find(".mceNoEditor").each(function () {
                    notIncludedRte = [];

                    // save the dragged RTE settings
                    draggedRteSettings = _.findWhere(tinyMCE.editors, { id: $(this).attr("id") }).settings;

                    // remove the dragged RTE
                    tinyMCE.execCommand("mceRemoveEditor", false, $(this).attr("id"));
                });
            },

            stop: function (e, ui) {
                ui.item.parents(".usky-cell").find(".mceNoEditor").each(function () {
                    if ($.inArray($(this).attr("id"), notIncludedRte) < 0) {
                        // add all dragged's neighbouring RTEs in the new cell
                        notIncludedRte.splice(0, 0, $(this).attr("id"));
                    }
                });
                $timeout(function () {
                    // reconstruct the dragged RTE (could be undefined when dragging something else than RTE)
                    if (draggedRteSettings !== undefined) {
                        tinyMCE.init(draggedRteSettings);
                    }

                    _.forEach(notIncludedRte, function (id) {
                        // reset all the other RTEs
                        if (draggedRteSettings === undefined || id !== draggedRteSettings.id) {
                            var rteSettings = _.findWhere(tinyMCE.editors, { id: id }).settings;
                            tinyMCE.execCommand("mceRemoveEditor", false, id);
                            tinyMCE.init(rteSettings);
                        }
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

        function getAllowedLayouts(section) {

            var layouts = $scope.model.config.items.layouts;

            //This will occur if it is a new section which has been
            // created from a 'template'
            if (section.allowed && section.allowed.length > 0) {
                return _.filter(layouts, function (layout) {
                    return _.indexOf(section.allowed, layout.name) >= 0;
                });
            }
            else {


                return layouts;
            }
        }

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

                //$scope.initContent();
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


        var guid = (function () {
            function s4() {
                return Math.floor((1 + Math.random()) * 0x10000)
                           .toString(16)
                           .substring(1);
            }
            return function () {
                return s4() + s4() + "-" + s4() + "-" + s4() + "-" +
                       s4() + "-" + s4() + s4() + s4();
            };
        })();

        $scope.setUniqueId = function (cell, index) {
            return guid();
        };

        $scope.addControl = function (editor, cell, index, initialize) {
            $scope.closeItemOverlay();
            initialize = (initialize !== false);

            var newControl = {
                value: null,
                editor: editor,
                $initializing: initialize
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
            return ((spans / $scope.model.config.items.columns) * 100).toFixed(8);
        };


        $scope.clearPrompt = function (scopedObject, e) {
            scopedObject.deletePrompt = false;
            e.preventDefault();
            e.stopPropagation();
        };

        $scope.showPrompt = function (scopedObject) {
            scopedObject.deletePrompt = true;
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
        $scope.initContent = function () {
            var clear = true;

            //settings indicator shortcut
            if ( ($scope.model.config.items.config && $scope.model.config.items.config.length > 0) || ($scope.model.config.items.styles && $scope.model.config.items.styles.length > 0)) {
                $scope.hasSettings = true;
            }

            //ensure the grid has a column value set,
            //if nothing is found, set it to 12
            if ($scope.model.config.items.columns && angular.isString($scope.model.config.items.columns)) {
                $scope.model.config.items.columns = parseInt($scope.model.config.items.columns);
            } else {
                $scope.model.config.items.columns = 12;
            }

            if ($scope.model.value && $scope.model.value.sections && $scope.model.value.sections.length > 0) {

                if ($scope.model.value.name && angular.isArray($scope.model.config.items.templates)) {

                    //This will occur if it is an existing value, in which case
                    // we need to determine which layout was applied by looking up
                    // the name
                    // TODO: We need to change this to an immutable ID!!

                    var found = _.find($scope.model.config.items.templates, function (t) {
                        return t.name === $scope.model.value.name;
                    });

                    if (found && angular.isArray(found.sections) && found.sections.length === $scope.model.value.sections.length) {

                        //Cool, we've found the template associated with our current value with matching sections counts, now we need to
                        // merge this template data on to our current value (as if it was new) so that we can preserve what is and isn't
                        // allowed for this template based on the current config.

                        _.each(found.sections, function (templateSection, index) {
                            angular.extend($scope.model.value.sections[index], angular.copy(templateSection));
                        });

                    }
                }

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
                clear = false;
            }

            if (clear) {
                $scope.model.value = undefined;
            }
        };

        $scope.initSection = function (section) {
            section.$percentage = $scope.percentage(section.grid);

            section.$allowedLayouts = getAllowedLayouts(section);

            if (!section.rows || section.rows.length === 0) {
                section.rows = [];
                if(section.$allowedLayouts.length === 1){
                    $scope.addRow(section, section.$allowedLayouts[0]);
                }
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

                        //copy over existing controls into the new areas
                        if (row.areas.length > areaIndex && row.areas[areaIndex].controls) {
                            area.controls = currentArea.controls;

                            _.forEach(area.controls, function (control, controlIndex) {
                                $scope.initControl(control, controlIndex);
                            });

                        } else {
                            //if empty
                            area.controls = [];

                            //if only one allowed editor
                            if(area.$allowedEditors.length === 1){
                                $scope.addControl(area.$allowedEditors[0], area, 0, false);
                            }
                        }

                        //set width
                        area.$percentage = $scope.percentage(area.grid);
                        area.$uniqueId = $scope.setUniqueId();

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

        //Clean the grid value before submitting to the server, we don't need
        // all of that grid configuration in the value to be stored!! All of that
        // needs to be merged in at runtime to ensure that the real config values are used
        // if they are ever updated.

        var unsubscribe = $scope.$on("formSubmitting", function () {

            if ($scope.model.value && $scope.model.value.sections) {
                _.each($scope.model.value.sections, function(section) {
                    if (section.rows) {
                        _.each(section.rows, function (row) {
                            if (row.areas) {
                                _.each(row.areas, function (area) {

                                    //Remove the 'editors' - these are the allowed editors, these will
                                    // be injected at runtime to this editor, it should not be persisted

                                    if (area.editors) {
                                        delete area.editors;
                                    }

                                    if (area.controls) {
                                        _.each(area.controls, function (control) {
                                            if (control.editor) {
                                                //replace
                                                var alias = control.editor.alias;
                                                control.editor = {
                                                    alias: alias
                                                };
                                            }
                                        });
                                    }
                                });
                            }
                        });
                    }
                });
            }
        });

        //when the scope is destroyed we need to unsubscribe
        $scope.$on("$destroy", function () {
            unsubscribe();
        });

    });
