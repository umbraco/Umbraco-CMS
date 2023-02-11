angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.GridController",
        function (
            $scope,
            localizationService,
            gridService,
            umbRequestHelper,
            angularHelper,
            $element,
            eventsService,
            editorService,
            overlayService,
            $interpolate
        ) {

            // Grid status variables
            var placeHolder = "";
            var currentForm = angularHelper.getCurrentForm($scope);

            $scope.currentRowWithActiveChild = null;
            $scope.currentCellWithActiveChild = null;
            $scope.active = null;

            $scope.currentRow = null;
            $scope.currentCell = null;
            $scope.currentToolsControl = null;
            $scope.currentControl = null;

            $scope.openRTEToolbarId = null;
            $scope.hasSettings = false;
            $scope.showRowConfigurations = true;
            $scope.sortMode = false;
            $scope.reorderKey = "general_reorder";

            // *********************************************
            // Sortable options
            // *********************************************

            var draggedRteSettings;// holds a dictionary of RTE settings to remember when dragging things around.

            $scope.sortableOptionsRow = {
                distance: 10,
                cursor: "move",
                placeholder: "ui-sortable-placeholder",
                handle: ".umb-row-title-bar",
                helper: "clone",
                forcePlaceholderSize: true,
                tolerance: "pointer",
                zIndex: 999999999999999999,
                scrollSensitivity: 100,
                disabled: $scope.readonly,
                cursorAt: {
                    top: 40,
                    left: 60
                },

                sort: function (event, ui) {
                    /* prevent vertical scroll out of the screen */
                    var max = $(".umb-grid").width() - 150;
                    if (parseInt(ui.helper.css("left")) > max) {
                        ui.helper.css({ "left": max + "px" });
                    }
                    if (parseInt(ui.helper.css("left")) < 20) {
                        ui.helper.css({ "left": 20 });
                    }
                },

                start: function (e, ui) {

                    // Fade out row when sorting
                    ui.item[0].style.display = "block";
                    ui.item[0].style.opacity = "0.5";

                    draggedRteSettings = {};
                    ui.item.find(".umb-rte").each(function (key, value) {
                        // remove all RTEs in the dragged row and save their settings
                        var rteId = value.id;
                        var editor = _.findWhere(tinyMCE.editors, { id: rteId });
                        if (editor) {
                            draggedRteSettings[rteId] = editor.settings;
                        }
                    });
                },

                stop: function (e, ui) {

                    // Fade in row when sorting stops
                    ui.item[0].style.opacity = "1";

                    // reset all RTEs affected by the dragging
                    ui.item.parents(".umb-column").find(".umb-rte").each(function (key, value) {
                        var rteId = value.id;
                        var settings = draggedRteSettings[rteId];
                        if (!settings) {
                            var editor = _.findWhere(tinyMCE.editors, { id: rteId });
                            if (editor) {
                                settings = editor.settings;
                            }
                        }
                        if (settings) {
                            tinyMCE.execCommand("mceRemoveEditor", false, rteId);
                            tinyMCE.init(settings);
                        }
                    });
                    currentForm.$setDirty();
                }
            };

            var notIncludedRte = [];// used for RTEs that has been affected by the sorting
            var cancelMove = false;
            var startingArea;
            $scope.sortableOptionsCell = {
                distance: 10,
                cursor: "move",
                uiFloating: true,
                placeholder: "ui-sortable-placeholder",
                handle: ".umb-control-handle",
                helper: "clone",
                connectWith: ".umb-cell-inner",
                forcePlaceholderSize: true,
                tolerance: "pointer",
                zIndex: 999999999999999999,
                scrollSensitivity: 100,
                disabled: $scope.readonly,
                cursorAt: {
                    top: 45,
                    left: 90
                },

                sort: function (event, ui) {

                    /* prevent vertical scroll out of the screen */
                    var position = parseInt(ui.item.parent().offset().left) + parseInt(ui.helper.css("left")) - parseInt($(".umb-grid").offset().left);
                    var max = $(".umb-grid").width() - 220;
                    if (position > max) {
                        ui.helper.css({ "left": max - parseInt(ui.item.parent().offset().left) + parseInt($(".umb-grid").offset().left) + "px" });
                    }
                    if (position < 0) {
                        ui.helper.css({ "left": 0 - parseInt(ui.item.parent().offset().left) + parseInt($(".umb-grid").offset().left) + "px" });
                    }
                },

                over: function (event, ui) {

                    var area = event.target.getScope_HackForSortable().area;
                    var allowedEditors = area.$allowedEditors.map(e => e.alias);

                    if (($.inArray(ui.item[0].getScope_HackForSortable().control.editor.alias, allowedEditors) < 0) ||
                        (startingArea != area && area.maxItems != '' && area.maxItems > 0 && area.maxItems < area.controls.length + 1)) {

                        $scope.$apply(function () {
                            area.dropNotAllowed = true;
                        });

                        ui.placeholder.hide();
                        cancelMove = true;
                    }
                    else {
                        if (area.controls.length == 0) {

                            $scope.$apply(function () {
                                area.dropOnEmpty = true;
                            });
                            ui.placeholder.hide();
                        } else {
                            ui.placeholder.show();
                        }
                        cancelMove = false;
                    }
                },

                out: function (event, ui) {
                    $scope.$apply(function () {
                        var dropArea = event.target.getScope_HackForSortable().area;
                        dropArea.dropNotAllowed = false;
                        dropArea.dropOnEmpty = false;
                    });
                },

                update: function (event, ui) {
                    /* add all RTEs which are affected by the dragging */
                    if (!ui.sender) {
                        if (cancelMove) {
                            ui.item.sortable.cancel();
                        }
                        ui.item.parents(".umb-cell-content").find(".umb-rte").each(function (key, value) {
                            var rteId = value.id;

                            if ($.inArray(rteId, notIncludedRte) < 0) {

                                // remember this RTEs settings, cause we need to update it later.
                                var editor = _.findWhere(tinyMCE.editors, { id: rteId })
                                if (editor) {
                                    draggedRteSettings[rteId] = editor.settings;
                                }
                                notIncludedRte.splice(0, 0, rteId);
                            }
                        });
                    }
                    else {
                        $(event.target).find(".umb-rte").each(function () {

                            var rteId = $(this).attr("id");

                            if ($.inArray(rteId, notIncludedRte) < 0) {

                                // remember this RTEs settings, cause we need to update it later.
                                var editor = _.findWhere(tinyMCE.editors, { id: rteId })
                                if (editor) {
                                    draggedRteSettings[rteId] = editor.settings;
                                }

                                notIncludedRte.splice(0, 0, $(this).attr("id"));
                            }
                        });
                    }
                    currentForm.$setDirty();
                },

                start: function (event, ui) {
                    //Get the starting area for reference
                    var area = event.target.getScope_HackForSortable().area;
                    startingArea = area;

                    // fade out control when sorting
                    ui.item[0].style.display = "block";
                    ui.item[0].style.opacity = "0.5";

                    // reset dragged RTE settings in case a RTE isn't dragged
                    draggedRteSettings = {};
                    notIncludedRte = [];

                    ui.item[0].style.display = "block";
                    ui.item.find(".umb-rte").each(function (key, value) {

                        var rteId = value.id;

                        // remember this RTEs settings, cause we need to update it later.
                        var editor = _.findWhere(tinyMCE.editors, { id: rteId });

                        // save the dragged RTE settings
                        if (editor) {
                            draggedRteSettings[rteId] = editor.settings;

                            // remove the dragged RTE
                            tinyMCE.execCommand("mceRemoveEditor", false, rteId);

                        }

                    });
                },

                stop: function (event, ui) {
                    // Fade in control when sorting stops
                    ui.item[0].style.opacity = "1";

                    ui.item.offsetParent().find(".umb-rte").each(function (key, value) {
                        var rteId = value.id;
                        if ($.inArray(rteId, notIncludedRte) < 0) {

                            var editor = _.findWhere(tinyMCE.editors, { id: rteId });
                            if (editor) {
                                draggedRteSettings[rteId] = editor.settings;
                            }

                            // add all dragged's neighbouring RTEs in the new cell
                            notIncludedRte.splice(0, 0, rteId);
                        }
                    });

                    // reconstruct the dragged RTE (could be undefined when dragging something else than RTE)
                    if (draggedRteSettings !== undefined) {
                        tinyMCE.init(draggedRteSettings);
                    }

                    _.forEach(notIncludedRte, function (rteId) {
                        // reset all the other RTEs
                        if (draggedRteSettings === undefined || rteId !== draggedRteSettings.id) {
                            tinyMCE.execCommand("mceRemoveEditor", false, rteId);
                            if (draggedRteSettings[rteId]) {
                                tinyMCE.init(draggedRteSettings[rteId]);
                            }
                        }
                    });

                    $scope.$apply(function () {
                        var cell = event.target.getScope_HackForSortable().area;

                        if (hasActiveChild(cell, cell.controls)) {
                            $scope.currentCellWithActiveChild = cell;
                        }
                        $scope.active = cell;

                    });
                }

            };

            $scope.toggleSortMode = function () {
                $scope.sortMode = !$scope.sortMode;
                if ($scope.sortMode) {
                    $scope.reorderKey = "general_reorderDone";
                } else {
                    $scope.reorderKey = "general_reorder";
                }
            };

            $scope.showReorderButton = function () {
                if ($scope.readonly) return false;

                if ($scope.model.value && $scope.model.value.sections) {
                    for (var i = 0; $scope.model.value.sections.length > i; i++) {
                        var section = $scope.model.value.sections[i];
                        if (section.rows && section.rows.length > 0) {
                            return true;
                        }
                    }
                }
            };

            // *********************************************
            // Add items overlay menu
            // *********************************************
            $scope.openEditorOverlay = function (event, area, index, key) {
                const dialog = {
                    view: "itempicker",
                    filter: area.$allowedEditors.length > 15,
                    availableItems: area.$allowedEditors,
                    event: event,
                    submit: function (model) {
                        if (model.selectedItem) {
                            $scope.addControl(model.selectedItem, area, index);
                            overlayService.close();
                        }
                    },
                    close: function () {
                        overlayService.close();
                    }
                };

                localizationService.localize("grid_insertControl").then(value => {
                    dialog.title = value;
                    overlayService.open(dialog);
                });
            };

            // *********************************************
            // Template management functions
            // *********************************************

            $scope.addTemplate = function (template) {
                $scope.model.value = Utilities.copy(template);

                //default row data
                _.forEach($scope.model.value.sections, function (section) {
                    $scope.initSection(section);
                });
            };


            // *********************************************
            // Row management function
            // *********************************************

            $scope.clickRow = function (index, rows, $event) {
                if ($scope.readonly) return;

                $scope.currentRowWithActiveChild = null;
                $scope.active = rows[index];

                $event.stopPropagation();
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

            $scope.addRow = function (section, layout, isInit) {

                //copy the selected layout into the rows collection
                var row = Utilities.copy(layout);

                // Init row value
                row = $scope.initRow(row);

                // Push the new row
                if (row) {
                    section.rows.push(row);
                }
                if (!isInit) {
                    currentForm.$setDirty();
                }

                $scope.showRowConfigurations = false;

                eventsService.emit("grid.rowAdded", { scope: $scope, element: $element, row: row });

                if (!isInit) {
                    // TODO: find a nicer way to do this without relying on setTimeout
                    setTimeout(function () {
                        var newRowEl = $element.find("[data-rowid='" + row.$uniqueId + "']");

                        if (newRowEl !== null) {
                            newRowEl.focus();
                        }
                    }, 0);
                }

            };

            $scope.removeRow = function (section, $index) {
                if (section.rows.length > 0) {
                    section.rows.splice($index, 1);
                    $scope.currentRow = null;
                    $scope.currentRowWithActiveChild = null;
                    $scope.openRTEToolbarId = null;
                    currentForm.$setDirty();
                }

                if (section.rows.length === 0) {
                    $scope.showRowConfigurations = true;
                }
            };

            var shouldApply = function (item, itemType, gridItem) {
                if (item.applyTo === undefined || item.applyTo === null || item.applyTo === "") {
                    return true;
                }

                if (typeof (item.applyTo) === "string") {
                    return item.applyTo === itemType;
                }

                if (itemType === "row") {
                    if (item.applyTo.row === undefined) {
                        return false;
                    }
                    if (item.applyTo.row === null || item.applyTo.row === "") {
                        return true;
                    }
                    var rows = item.applyTo.row.split(',');
                    return _.indexOf(rows, gridItem.name) !== -1;
                } else if (itemType === "cell") {
                    if (item.applyTo.cell === undefined) {
                        return false;
                    }
                    if (item.applyTo.cell === null || item.applyTo.cell === "") {
                        return true;
                    }
                    var cells = item.applyTo.cell.split(',');
                    var cellSize = gridItem.grid.toString();
                    return _.indexOf(cells, cellSize) !== -1;
                }
            }

            $scope.editGridItemSettings = function (gridItem, itemType) {

                placeHolder = "{0}";

                var styles, config;
                if (itemType === 'control') {
                    styles = null;
                    config = Utilities.copy(gridItem.editor.config.settings);
                } else {
                    styles = _.filter(Utilities.copy($scope.model.config.items.styles), function (item) { return shouldApply(item, itemType, gridItem); });
                    config = _.filter(Utilities.copy($scope.model.config.items.config), function (item) { return shouldApply(item, itemType, gridItem); });
                }

                if (Utilities.isObject(gridItem.config)) {
                    _.each(config, function (cfg) {
                        var val = gridItem.config[cfg.key];
                        if (val) {
                            cfg.value = stripModifier(val, cfg.modifier);
                        }
                    });
                }

                if (Utilities.isObject(gridItem.styles)) {
                    _.each(styles, function (style) {
                        var val = gridItem.styles[style.key];
                        if (val) {
                            style.value = stripModifier(val, style.modifier);
                        }
                    });
                }

                var dialogOptions = {
                    view: "views/propertyeditors/grid/dialogs/config.html",
                    size: "small",
                    styles: styles,
                    config: config,
                    submit: function (model) {
                        var styleObject = {};
                        var configObject = {};

                        _.each(model.styles, function (style) {
                            if (style.value) {
                                styleObject[style.key] = addModifier(style.value, style.modifier);
                            }
                        });
                        _.each(model.config, function (cfg) {
                            cfg.alias = cfg.key;
                            cfg.label = cfg.value;

                            if (cfg.value) {
                                configObject[cfg.key] = addModifier(cfg.value, cfg.modifier);
                            }
                        });

                        gridItem.styles = styleObject;
                        gridItem.config = configObject;
                        gridItem.hasConfig = gridItemHasConfig(styleObject, configObject);

                        currentForm.$setDirty();

                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };

                localizationService.localize("general_settings").then(value => {
                    dialogOptions.title = value;
                    editorService.open(dialogOptions);
                });

            };

            function stripModifier(val, modifier) {
                if (!val || !modifier || modifier.indexOf(placeHolder) < 0) {
                    return val;
                } else {
                    var paddArray = modifier.split(placeHolder);
                    if (paddArray.length == 1) {
                        if (modifier.indexOf(placeHolder) === 0) {
                            return val.slice(0, -paddArray[0].length);
                        } else {
                            return val.slice(paddArray[0].length, 0);
                        }
                    } else {
                        if (paddArray[1].length === 0) {
                            return val.slice(paddArray[0].length);
                        }
                        return val.slice(paddArray[0].length, -paddArray[1].length);
                    }
                }
            }

            var addModifier = function (val, modifier) {
                if (!modifier || modifier.indexOf(placeHolder) < 0) {
                    return val;
                } else {
                    return modifier.replace(placeHolder, val);
                }
            };

            function gridItemHasConfig(styles, config) {

                if (_.isEmpty(styles) && _.isEmpty(config)) {
                    return false;
                } else {
                    return true;
                }

            }

            // *********************************************
            // Area management functions
            // *********************************************

            $scope.clickCell = function (index, cells, row, $event) {
                if ($scope.readonly) return;

                $scope.currentCellWithActiveChild = null;

                $scope.active = cells[index];
                $scope.currentRowWithActiveChild = row;
                $event.stopPropagation();
            };

            $scope.cellPreview = function (cell) {
                if (cell && cell.$allowedEditors) {
                    var editor = cell.$allowedEditors[0];
                    return editor.icon;
                } else {
                    return "icon-layout";
                }
            };


            // *********************************************
            // Control management functions
            // *********************************************
            $scope.clickControl = function (index, controls, cell, $event) {
                if ($scope.readonly) return;

                $scope.active = controls[index];
                $scope.currentCellWithActiveChild = cell;

                $event.stopPropagation();
            };

            function hasActiveChild(item, children) {

                var activeChild = false;

                for (var i = 0; children.length > i; i++) {
                    var child = children[i];

                    if (child.active) {
                        activeChild = true;
                    }
                }

                if (activeChild) {
                    return true;
                }

            }

            $scope.setUniqueId = function () {
                return String.CreateGuid();
            };

            $scope.addControl = function (editor, cell, index, initialize) {

                initialize = (initialize !== false);

                var newControl = {
                    value: null,
                    editor: editor,
                    $initializing: initialize
                };

                if (index === undefined) {
                    index = cell.controls.length;
                }

                $scope.active = newControl;

                //populate control
                $scope.initControl(newControl, index + 1);

                cell.controls.push(newControl);

                eventsService.emit("grid.itemAdded", { scope: $scope, element: $element, cell: cell, item: newControl });

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
                currentForm.$setDirty();
            };

            $scope.percentage = function (spans) {
                return ((spans / $scope.model.config.items.columns) * 100).toFixed(8);
            };


            $scope.clearPrompt = function (scopedObject, e) {
                scopedObject.deletePrompt = false;
                e.preventDefault();
                e.stopPropagation();
            };

            $scope.togglePrompt = function (scopedObject) {
                scopedObject.deletePrompt = !scopedObject.deletePrompt;
            };

            $scope.hidePrompt = function (scopedObject) {
                scopedObject.deletePrompt = false;
            };

            $scope.toggleAddRow = function () {
                $scope.showRowConfigurations = !$scope.showRowConfigurations;
            };

            $scope.getTemplateName = function (control) {
                var templateName = control.editor.name;
                if (control.editor.nameExp) {
                    var valueOfTemplate = control.editor.nameExp(control);
                    if (valueOfTemplate != "") {
                        templateName += ": ";
                        templateName += valueOfTemplate;
                    }
                }
                return templateName;
            }

            // *********************************************
            // Initialization
            // these methods are called from ng-init on the template
            // so we can controll their first load data
            //
            // intialization sets non-saved data like percentage sizing, allowed editors and
            // other data that should all be pre-fixed with $ to strip it out on save
            // *********************************************

            // *********************************************
            // Init template + sections
            // *********************************************
            $scope.initContent = function () {
                var clear = true;

                //settings indicator shortcut
                if (($scope.model.config.items.config && $scope.model.config.items.config.length > 0) || ($scope.model.config.items.styles && $scope.model.config.items.styles.length > 0)) {
                    $scope.hasSettings = true;
                }

                //ensure the grid has a column value set,
                //if nothing is found, set it to 12
                if (!$scope.model.config.items.columns) {
                    $scope.model.config.items.columns = 12;
                } else if (Utilities.isString($scope.model.config.items.columns)) {
                    $scope.model.config.items.columns = parseInt($scope.model.config.items.columns);
                }

                if ($scope.model.value && $scope.model.value.sections && $scope.model.value.sections.length > 0 && $scope.model.value.sections[0].rows && $scope.model.value.sections[0].rows.length > 0) {

                    if ($scope.model.value.name && Utilities.isArray($scope.model.config.items.templates)) {

                        //This will occur if it is an existing value, in which case
                        // we need to determine which layout was applied by looking up
                        // the name
                        // TODO: We need to change this to an immutable ID!!

                        var found = _.find($scope.model.config.items.templates, function (t) {
                            return t.name === $scope.model.value.name;
                        });

                        if (found && Utilities.isArray(found.sections) && found.sections.length === $scope.model.value.sections.length) {

                            //Cool, we've found the template associated with our current value with matching sections counts, now we need to
                            // merge this template data on to our current value (as if it was new) so that we can preserve what is and isn't
                            // allowed for this template based on the current config.

                            _.each(found.sections, function (templateSection, index) {
                                Utilities.extend($scope.model.value.sections[index], Utilities.copy(templateSection));
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
                    if (section.$allowedLayouts.length === 1) {
                        $scope.addRow(section, section.$allowedLayouts[0], true);
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

                    // if there is more than one row added - hide row add tools
                    $scope.showRowConfigurations = false;
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
                    original = Utilities.copy(original);
                    original.styles = row.styles;
                    original.config = row.config;
                    original.hasConfig = gridItemHasConfig(row.styles, row.config);


                    //sync area configuration
                    _.each(original.areas, function (area, areaIndex) {


                        if (area.grid > 0) {
                            var currentArea = row.areas[areaIndex];

                            if (currentArea) {
                                area.config = currentArea.config;
                                area.styles = currentArea.styles;
                                area.hasConfig = gridItemHasConfig(currentArea.styles, currentArea.config);
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
                                if (area.$allowedEditors.length === 1) {
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

                //Localize the grid editor names
                $scope.availableEditors.forEach(function (value) {
                    //If no translation is provided, keep using the editor name from the manifest
                    localizationService.localize("grid_" + value.alias, undefined, value.name).then(function (v) {
                        value.name = v;
                    });
                    // setup nametemplate

                    value.nameExp = !!value.nameTemplate
                        ? $interpolate(value.nameTemplate)
                        : undefined;
                });

                $scope.contentReady = true;

                // *********************************************
                // Init grid
                // *********************************************

                eventsService.emit("grid.initializing", { scope: $scope, element: $element });

                $scope.initContent();

                eventsService.emit("grid.initialized", { scope: $scope, element: $element });

            });

            //Clean the grid value before submitting to the server, we don't need
            // all of that grid configuration in the value to be stored!! All of that
            // needs to be merged in at runtime to ensure that the real config values are used
            // if they are ever updated.

            var unsubscribe = $scope.$on("formSubmitting", function (e, args) {

                if (args.action === "save" && $scope.model.value && $scope.model.value.sections) {
                    _.each($scope.model.value.sections, function (section) {
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
