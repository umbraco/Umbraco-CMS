angular.module("umbraco").controller("Umbraco.PropertyEditors.NestedContentController", [

    "$scope",
    "$interpolate",
    "$filter",
    "$timeout",
    "contentResource",
    "localizationService",

    function ($scope, $interpolate, $filter, $timeout, contentResource, localizationService) {

        var inited = false;

        _.each($scope.model.config.contentTypes, function (contentType) {
            contentType.nameExp = !!contentType.nameTemplate
                ? $interpolate(contentType.nameTemplate)
                : undefined;
        });

        $scope.deleteIconTitle = '';
        $scope.moveIconTitle = '';

        // localize the delete icon title
        localizationService.localize('general_delete').then(function (value) {
            $scope.deleteIconTitle = value;
        });

        localizationService.localize('general_move').then(function (value) {
            $scope.moveIconTitle = value;
        });

        $scope.nodes = [];
        $scope.currentNode = undefined;
        $scope.realCurrentNode = undefined;
        $scope.scaffolds = undefined;
        $scope.sorting = false;
        $scope.deletePromptIndex = undefined;

        $scope.minItems = $scope.model.config.minItems || 0;
        $scope.maxItems = $scope.model.config.maxItems || 0;

        if ($scope.maxItems == 0)
            $scope.maxItems = 1000;

        $scope.singleMode = $scope.minItems == 1 && $scope.maxItems == 1;
        $scope.wideMode = $scope.model.config.hideLabel == "1";

        $scope.addNode = function (alias) {
            var scaffold = $scope.getScaffold(alias);
            var newNode = initNode(scaffold, null);
            $scope.currentNode = newNode;
            $scope.closeNodeTypePickerOverlay();
        };

        $scope.openNodeTypePickerOverlay = function (event) {
            if ($scope.nodes.length >= $scope.maxItems) {
                return;
            }

            // this could be used for future limiting on node types
            var scaffolds = [];
            _.each($scope.scaffolds, function (scaffold) {
                var icon = scaffold.icon;
                // workaround for when no icon is chosen for a doctype
                if (icon === ".sprTreeFolder") {
                    icon = "icon-folder";
                }
                scaffolds.push({
                    alias: scaffold.contentTypeAlias,
                    name: scaffold.contentTypeName,
                    icon: icon
                });
            });

            if (scaffolds.length == 0) {
                return;
            }

            if (scaffolds.length == 1) {
                // only one scaffold type - no need to display the picker
                $scope.addNode(scaffolds[0].alias);
                return;
            }

            $scope.nodeTypePickerOverlay = {
                view: "itempicker",
                filter: false,
                title: localizationService.localize("grid_insertControl"), // Should probably use a NC specific string, but for now re-using the grid title
                availableItems: scaffolds,
                event: event,
                show: true,
                submit: function (model) {
                    $scope.addNode(model.selectedItem.alias);
                }
            };
        };

        $scope.closeNodeTypePickerOverlay = function () {
            if ($scope.nodeTypePickerOverlay) {
                $scope.nodeTypePickerOverlay.show = false;
                $scope.nodeTypePickerOverlay = null;
            }
        };

        $scope.editNode = function (idx) {
            if ($scope.sorting) return;
            if ($scope.currentNode && $scope.currentNode.key == $scope.nodes[idx].key) {
                $scope.currentNode = undefined;
            } else {
                $scope.currentNode = $scope.nodes[idx];
            }
        };

        $scope.showDeletePrompt = function (idx) {
            $scope.deletePromptIndex = idx;
        }

        $scope.confirmDelete = function () {
            if ($scope.nodes.length > $scope.model.config.minItems) {
                $scope.nodes.splice($scope.deletePromptIndex, 1);
                updateModel();
            }
            $scope.deletePromptIndex = undefined;
        }

        $scope.hideDeletePrompt = function () {
            $scope.deletePromptIndex = undefined;
        }

        $scope.getName = function (idx) {

            var name = "Item " + (idx + 1);

            if ($scope.model.value[idx]) {

                var contentType = $scope.getContentTypeConfig($scope.model.value[idx].ncContentTypeAlias);

                if (contentType != null && contentType.nameExp) {
                    var newName = contentType.nameExp($scope.model.value[idx]); // Run it against the stored dictionary value, NOT the node object
                    if (newName && (newName = $.trim(newName))) {
                        name = newName;
                    }
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
            return scaffold && scaffold.icon && scaffold.icon != ".sprTreeFolder" ? scaffold.icon : "icon-folder";
        }

        $scope.sortableOptions = {
            axis: 'y',
            cursor: "move",
            handle: ".nested-content__icon--move",
            forceHelperSize: true,
            start: function (ev, ui) {
                // Yea, yea, we shouldn't modify the dom, sue me
                $("#nested-content--" + $scope.model.id + " .umb-rte textarea").each(function () {
                    tinymce.execCommand('mceRemoveEditor', false, $(this).attr('id'));
                    $(this).css("visibility", "hidden");
                });
                $scope.$apply(function () {
                    $scope.sorting = true;
                });
            },
            stop: function (ev, ui) {
                $("#nested-content--" + $scope.model.id + " .umb-rte textarea").each(function () {
                    tinymce.execCommand('mceAddEditor', true, $(this).attr('id'));
                    $(this).css("visibility", "visible");
                });
                $scope.$apply(function () {
                    updateModel();
                });
                $timeout(function () {
                    $scope.sorting = false;
                }, 250);
            }
        };

        $scope.getScaffold = function (alias) {
            return _.find($scope.scaffolds, function (scaffold) {
                return scaffold.contentTypeAlias == alias;
            });
        }

        $scope.getContentTypeConfig = function (alias) {
            return _.find($scope.model.config.contentTypes, function (contentType) {
                return contentType.ncAlias == alias;
            });
        }

        // Initialize
        var scaffoldsLoaded = 0;
        $scope.scaffolds = [];
        _.each($scope.model.config.contentTypes, function (contentType) {
            contentResource.getScaffold(-20, contentType.ncAlias).then(function (scaffold) {
                // remove all tabs except the specified tab
                var tab = _.find(scaffold.tabs, function (tab) {
                    return tab.id != 0 && (tab.alias.toLowerCase() == contentType.ncTabAlias.toLowerCase() || contentType.ncTabAlias == "");
                });
                scaffold.tabs = [];
                if (tab) {
                    scaffold.tabs.push(tab);
                }

                // Store the scaffold object
                $scope.scaffolds.push(scaffold);

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
                $scope.scaffolds = $filter('orderBy')($scope.scaffolds, function (s) {
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
                if ($scope.singleMode) {
                    $scope.currentNode = $scope.nodes[0];
                }

                inited = true;
            }
        }

        var initNode = function (scaffold, item) {
            var node = angular.copy(scaffold);

            node.key = item  && item.key ? item.key : UUID.generate();
            node.ncContentTypeAlias = scaffold.contentTypeAlias;

            for (var t = 0; t < node.tabs.length; t++) {
                var tab = node.tabs[t];
                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];
                    prop.propertyAlias = prop.alias;
                    prop.alias = $scope.model.alias + "___" + prop.alias;
                    // Force validation to occur server side as this is the 
                    // only way we can have consistancy between mandatory and
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

        $scope.$on('$destroy', function () {
            unsubscribe();
        });

        //TODO: Move this into a shared location?
        var UUID = (function () {
            var self = {};
            var lut = []; for (var i = 0; i < 256; i++) { lut[i] = (i < 16 ? '0' : '') + (i).toString(16); }
            self.generate = function () {
                var d0 = Math.random() * 0xffffffff | 0;
                var d1 = Math.random() * 0xffffffff | 0; 
                var d2 = Math.random() * 0xffffffff | 0;
                var d3 = Math.random() * 0xffffffff | 0;
                return lut[d0 & 0xff] + lut[d0 >> 8 & 0xff] + lut[d0 >> 16 & 0xff] + lut[d0 >> 24 & 0xff] + '-' +
                  lut[d1 & 0xff] + lut[d1 >> 8 & 0xff] + '-' + lut[d1 >> 16 & 0x0f | 0x40] + lut[d1 >> 24 & 0xff] + '-' +
                  lut[d2 & 0x3f | 0x80] + lut[d2 >> 8 & 0xff] + '-' + lut[d2 >> 16 & 0xff] + lut[d2 >> 24 & 0xff] +
                  lut[d3 & 0xff] + lut[d3 >> 8 & 0xff] + lut[d3 >> 16 & 0xff] + lut[d3 >> 24 & 0xff];
            }
            return self;
        })();
    }

]);