angular.module("umbraco").controller("Umbraco.PropertyEditors.NestedContent.DocTypePickerController", [

    "$scope",
    "Umbraco.PropertyEditors.NestedContent.Resources",
    "overlayService",
    "iconHelper",

    function ($scope, ncResources, overlayService, iconHelper) {
        $scope.elemTypeTabs = [];


        init();


        function init() {

            ncResources.getContentTypes().then(function (elemTypes) {
                $scope.model.elemTypes = elemTypes;

                // convert legacy icons
                iconHelper.formatContentTypeIcons($scope.model.elemTypes);

                // Populate document type tab dictionary
                elemTypes.forEach(function (value) {
                    $scope.elemTypeTabs[value.alias] = value.tabs;
                });          
            });

        }


        $scope.add = function () {
            $scope.model.value.push({
                // As per PR #4, all stored content type aliases must be prefixed "nc" for easier recognition.
                // For good measure we'll also prefix the tab alias "nc"
                ncAlias: "",
                ncTabAlias: "",
                nameTemplate: ""           
            });
        }

        $scope.remove = function (index) {
            $scope.model.value.splice(index, 1);
        }

        $scope.sortableOptions = {
            axis: "y",
            cursor: "move",
            handle: ".handle",
            placeholder: 'sortable-placeholder',
            forcePlaceholderSize: true,
            helper: function (e, ui) {
                // When sorting table rows, the cells collapse. This helper fixes that: https://www.foliotek.com/devblog/make-table-rows-sortable-using-jquery-ui-sortable/
                ui.children().each(function () {
                    $(this).width($(this).width());
                });
                return ui;
            },
            start: function (e, ui) {

                var cellHeight = ui.item.height();

                // Build a placeholder cell that spans all the cells in the row: https://stackoverflow.com/questions/25845310/jquery-ui-sortable-and-table-cell-size
                var cellCount = 0;
                $('td, th', ui.helper).each(function () {
                    // For each td or th try and get it's colspan attribute, and add that or 1 to the total
                    var colspan = 1;
                    var colspanAttr = $(this).attr('colspan');
                    if (colspanAttr > 1) {
                        colspan = colspanAttr;
                    }
                    cellCount += colspan;
                });

                // Add the placeholder UI - note that this is the item's content, so td rather than tr - and set height of tr
                ui.placeholder.html('<td colspan="' + cellCount + '"></td>').height(cellHeight);
            }
        };

        $scope.placeholder = function (config) {
            return _.find($scope.model.elemTypes, function (elType) {
                return elType.alias === config.ncAlias;
            });
        }

        $scope.selectableElemTypesFor = function (config) {
            // return all elemTypes that are:
            // 1. either already selected for this config, or
            // 2. not selected in any other config
            return _.filter($scope.model.elemTypes, function (elType) {
                return elType.alias === config.ncAlias || !_.find($scope.model.value, function (c) {
                    return elType.alias === c.ncAlias;
                });
            });

        }


        $scope.openElemTypeModal = function ($event, config) {
            var elemTypeSelectorOverlay = {
                view: "itempicker",
                title: "Add Element Type",
                availableItems: $scope.selectableElemTypesFor(config),
                selectedItems: [],
                position: "target",
                event: $event,
                submit: function (model) {
                    config.ncAlias = model.selectedItem.alias;                   
                    overlayService.close();
                },
                close: function () {
                    overlayService.close();
                }
            };

            overlayService.open(elemTypeSelectorOverlay);
        }



        if (!$scope.model.value) {
            $scope.model.value = [];
            $scope.add();
        }

    }
]);

angular.module("umbraco").controller("Umbraco.PropertyEditors.NestedContent.PropertyEditorController", [

    "$scope",
    "$interpolate",
    "$filter",
    "$timeout",
    "contentResource",
    "localizationService",
    "iconHelper",

    function ($scope, $interpolate, $filter, $timeout, contentResource, localizationService, iconHelper) {

        var inited = false;

        _.each($scope.model.config.contentTypes, function (contentType) {
            contentType.nameExp = !!contentType.nameTemplate
                ? $interpolate(contentType.nameTemplate)
                : undefined;
        });

        $scope.nodes = [];
        $scope.currentNode = undefined;
        $scope.realCurrentNode = undefined;
        $scope.scaffolds = undefined;
        $scope.sorting = false;

        $scope.minItems = $scope.model.config.minItems || 0;
        $scope.maxItems = $scope.model.config.maxItems || 0;

        if ($scope.maxItems === 0)
            $scope.maxItems = 1000;

        $scope.singleMode = $scope.minItems === 1 && $scope.maxItems === 1;
        $scope.showIcons = Object.toBoolean($scope.model.config.showIcons);
        $scope.wideMode = Object.toBoolean($scope.model.config.hideLabel);
        $scope.hasContentTypes = $scope.model.config.contentTypes.length > 0;

        $scope.labels = {};
        localizationService.localizeMany(["grid_insertControl"]).then(function (data) {
            $scope.labels.docTypePickerTitle = data[0];
        });

        // helper to force the current form into the dirty state
        $scope.setDirty = function () {
            if ($scope.propertyForm) {
                $scope.propertyForm.$setDirty();
            }
        };

        $scope.addNode = function (alias) {
            var scaffold = $scope.getScaffold(alias);

            var newNode = initNode(scaffold, null);

            $scope.currentNode = newNode;
            $scope.setDirty();
        };

        $scope.openNodeTypePicker = function ($event) {
            if ($scope.nodes.length >= $scope.maxItems) {
                return;
            }

            $scope.overlayMenu = {
                title: $scope.labels.docTypePickerTitle,
                show: false,
                style: {},
                filter: $scope.scaffolds.length > 15 ? true : false,
                orderBy: "$index",
                view: "itempicker",
                event: $event,
                submit: function (model) {
                    if (model && model.selectedItem) {
                        $scope.addNode(model.selectedItem.alias);
                    }
                    $scope.overlayMenu.show = false;
                    $scope.overlayMenu = null;
                },
                close: function () {
                    $scope.overlayMenu.show = false;
                    $scope.overlayMenu = null;
                }
            };

            // this could be used for future limiting on node types
            $scope.overlayMenu.availableItems = [];
            _.each($scope.scaffolds, function (scaffold) {
                $scope.overlayMenu.availableItems.push({
                    alias: scaffold.contentTypeAlias,
                    name: scaffold.contentTypeName,
                    icon: iconHelper.convertFromLegacyIcon(scaffold.icon)
                });
            });

            if ($scope.overlayMenu.availableItems.length === 0) {
                return;
            }

            if ($scope.overlayMenu.availableItems.length === 1) {
                // only one scaffold type - no need to display the picker
                $scope.addNode($scope.scaffolds[0].contentTypeAlias);
                return;
            }

            $scope.overlayMenu.show = true;
        };

        $scope.editNode = function (idx) {
            if ($scope.currentNode && $scope.currentNode.key === $scope.nodes[idx].key) {
                $scope.currentNode = undefined;
            } else {
                $scope.currentNode = $scope.nodes[idx];
            }
        };

        $scope.deleteNode = function (idx) {
            if ($scope.nodes.length > $scope.model.config.minItems) {
                if ($scope.model.config.confirmDeletes && $scope.model.config.confirmDeletes === 1) {
                    localizationService.localize("content_nestedContentDeleteItem").then(function (value) {
                        if (confirm(value)) {
                            $scope.nodes.splice(idx, 1);
                            $scope.setDirty();
                            updateModel();
                        }
                    });
                } else {
                    $scope.nodes.splice(idx, 1);
                    $scope.setDirty();
                    updateModel();
                }
            }
        };

        $scope.getName = function (idx) {

            var name = "Item " + (idx + 1);

            if ($scope.model.value[idx]) {

                var contentType = $scope.getContentTypeConfig($scope.model.value[idx].ncContentTypeAlias);

                if (contentType != null && contentType.nameExp) {
                    // Run the expression against the stored dictionary value, NOT the node object
                    var item = $scope.model.value[idx];

                    // Add a temporary index property
                    item["$index"] = (idx + 1);

                    var newName = contentType.nameExp(item);
                    if (newName && (newName = $.trim(newName))) {
                        name = newName;
                    }

                    // Delete the index property as we don't want to persist it
                    delete item["$index"];
                }

            }

            // Update the nodes actual name value
            if ($scope.nodes[idx].name !== name) {
                $scope.nodes[idx].name = name;
            }


            return name;
        };

        $scope.getIcon = function (idx) {
            var scaffold = $scope.getScaffold($scope.model.value[idx].ncContentTypeAlias);
            return scaffold && scaffold.icon ? iconHelper.convertFromLegacyIcon(scaffold.icon) : "icon-folder";
        }

        $scope.sortableOptions = {
            axis: "y",
            cursor: "move",
            handle: ".umb-nested-content__icon--move",
            start: function (ev, ui) {
                updateModel();
                // Yea, yea, we shouldn't modify the dom, sue me
                $("#umb-nested-content--" + $scope.model.id + " .umb-rte textarea").each(function () {
                    tinymce.execCommand("mceRemoveEditor", false, $(this).attr("id"));
                    $(this).css("visibility", "hidden");
                });
                $scope.$apply(function () {
                    $scope.sorting = true;
                });
            },
            update: function (ev, ui) {
                $scope.setDirty();
            },
            stop: function (ev, ui) {
                $("#umb-nested-content--" + $scope.model.id + " .umb-rte textarea").each(function () {
                    tinymce.execCommand("mceAddEditor", true, $(this).attr("id"));
                    $(this).css("visibility", "visible");
                });
                $scope.$apply(function () {
                    $scope.sorting = false;
                    updateModel();
                });
            }
        };

        $scope.getScaffold = function (alias) {
            return _.find($scope.scaffolds, function (scaffold) {
                return scaffold.contentTypeAlias === alias;
            });
        }

        $scope.getContentTypeConfig = function (alias) {
            return _.find($scope.model.config.contentTypes, function (contentType) {
                return contentType.ncAlias === alias;
            });
        }

        var notSupported = [
            "Umbraco.Tags",
            "Umbraco.UploadField",
            "Umbraco.ImageCropper"
        ];

        // Initialize
        var scaffoldsLoaded = 0;
        $scope.scaffolds = [];
        _.each($scope.model.config.contentTypes, function (contentType) {
            contentResource.getScaffold(-20, contentType.ncAlias).then(function (scaffold) {
                // make sure it's an element type before allowing the user to create new ones
                if (scaffold.isElement) {
                    // remove all tabs except the specified tab
                    var tabs = scaffold.variants[0].tabs;
                    var tab = _.find(tabs, function (tab) {
                        return tab.id !== 0 && (tab.alias.toLowerCase() === contentType.ncTabAlias.toLowerCase() || contentType.ncTabAlias === "");
                    });
                    scaffold.tabs = [];
                    if (tab) {
                        scaffold.tabs.push(tab);

                        angular.forEach(tab.properties,
                            function (property) {
                                if (_.find(notSupported, function (x) { return x === property.editor; })) {
                                    property.notSupported = true;
                                    // TODO: Not supported message to be replaced with 'content_nestedContentEditorNotSupported' dictionary key. Currently not possible due to async/timing quirk.
                                    property.notSupportedMessage = "Property " + property.label + " uses editor " + property.editor + " which is not supported by Nested Content.";
                                }
                            });
                    }

                    // Store the scaffold object
                    $scope.scaffolds.push(scaffold);
                }

                scaffoldsLoaded++;
                initIfAllScaffoldsHaveLoaded();
            }, function (error) {
                scaffoldsLoaded++;
                initIfAllScaffoldsHaveLoaded();
            });
        });

        var initIfAllScaffoldsHaveLoaded = function () {
            // Initialize when all scaffolds have loaded
            if ($scope.model.config.contentTypes.length === scaffoldsLoaded) {
                // Because we're loading the scaffolds async one at a time, we need to
                // sort them explicitly according to the sort order defined by the data type.
                var contentTypeAliases = [];
                _.each($scope.model.config.contentTypes, function (contentType) {
                    contentTypeAliases.push(contentType.ncAlias);
                });
                $scope.scaffolds = $filter("orderBy")($scope.scaffolds, function (s) {
                    return contentTypeAliases.indexOf(s.contentTypeAlias);
                });

                // Convert stored nodes
                if ($scope.model.value) {
                    for (var i = 0; i < $scope.model.value.length; i++) {
                        var item = $scope.model.value[i];
                        var scaffold = $scope.getScaffold(item.ncContentTypeAlias);
                        if (scaffold == null) {
                            // No such scaffold - the content type might have been deleted. We need to skip it.
                            continue;
                        }
                        initNode(scaffold, item);
                    }
                }

                // Enforce min items
                if ($scope.nodes.length < $scope.model.config.minItems) {
                    for (var i = $scope.nodes.length; i < $scope.model.config.minItems; i++) {
                        $scope.addNode($scope.scaffolds[0].contentTypeAlias);
                    }
                }

                // If there is only one item, set it as current node
                if ($scope.singleMode || ($scope.nodes.length === 1 && $scope.maxItems === 1)) {
                    $scope.currentNode = $scope.nodes[0];
                }

                inited = true;
            }
        }

        var initNode = function (scaffold, item) {
            var node = angular.copy(scaffold);

            node.key = item && item.key ? item.key : UUID.generate();
            node.ncContentTypeAlias = scaffold.contentTypeAlias;

            for (var t = 0; t < node.tabs.length; t++) {
                var tab = node.tabs[t];
                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];
                    prop.propertyAlias = prop.alias;
                    prop.alias = $scope.model.alias + "___" + prop.alias;
                    // Force validation to occur server side as this is the
                    // only way we can have consistency between mandatory and
                    // regex validation messages. Not ideal, but it works.
                    prop.validation = {
                        mandatory: false,
                        pattern: ""
                    };
                    if (item) {
                        if (item[prop.propertyAlias]) {
                            prop.value = item[prop.propertyAlias];
                        }
                    }
                }
            }

            $scope.nodes.push(node);

            return node;
        }

        var updateModel = function () {
            if ($scope.realCurrentNode) {
                $scope.$broadcast("ncSyncVal", { key: $scope.realCurrentNode.key });
            }
            if (inited) {
                var newValues = [];
                for (var i = 0; i < $scope.nodes.length; i++) {
                    var node = $scope.nodes[i];
                    var newValue = {
                        key: node.key,
                        name: node.name,
                        ncContentTypeAlias: node.ncContentTypeAlias
                    };
                    for (var t = 0; t < node.tabs.length; t++) {
                        var tab = node.tabs[t];
                        for (var p = 0; p < tab.properties.length; p++) {
                            var prop = tab.properties[p];
                            if (typeof prop.value !== "function") {
                                newValue[prop.propertyAlias] = prop.value;
                            }
                        }
                    }
                    newValues.push(newValue);
                }
                $scope.model.value = newValues;
            }
        }

        $scope.$watch("currentNode", function (newVal) {
            updateModel();
            $scope.realCurrentNode = newVal;
        });

        var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
            updateModel();
        });

        $scope.$on("$destroy", function () {
            unsubscribe();
        });

        // TODO: Move this into a shared location?
        var UUID = (function () {
            var self = {};
            var lut = []; for (var i = 0; i < 256; i++) { lut[i] = (i < 16 ? "0" : "") + (i).toString(16); }
            self.generate = function () {
                var d0 = Math.random() * 0xffffffff | 0;
                var d1 = Math.random() * 0xffffffff | 0;
                var d2 = Math.random() * 0xffffffff | 0;
                var d3 = Math.random() * 0xffffffff | 0;
                return lut[d0 & 0xff] + lut[d0 >> 8 & 0xff] + lut[d0 >> 16 & 0xff] + lut[d0 >> 24 & 0xff] + "-" +
                    lut[d1 & 0xff] + lut[d1 >> 8 & 0xff] + "-" + lut[d1 >> 16 & 0x0f | 0x40] + lut[d1 >> 24 & 0xff] + "-" +
                    lut[d2 & 0x3f | 0x80] + lut[d2 >> 8 & 0xff] + "-" + lut[d2 >> 16 & 0xff] + lut[d2 >> 24 & 0xff] +
                    lut[d3 & 0xff] + lut[d3 >> 8 & 0xff] + lut[d3 >> 16 & 0xff] + lut[d3 >> 24 & 0xff];
            }
            return self;
        })();
    }

]);
