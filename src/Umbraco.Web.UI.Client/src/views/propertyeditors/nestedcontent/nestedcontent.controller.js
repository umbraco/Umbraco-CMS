angular.module("umbraco").controller("Umbraco.PropertyEditors.NestedContentController", [

    "$scope",
    "$interpolate",
    "$filter",
    "contentResource",
    "localizationService",

    function ($scope, $interpolate, $filter, contentResource, localizationService) {

        //$scope.model.config.contentTypes;
        //$scope.model.config.minItems;
        //$scope.model.config.maxItems;
        //console.log($scope);

        var inited = false;

        _.each($scope.model.config.contentTypes, function (contentType) {
            contentType.nameExp = !!contentType.nameTemplate
                ? $interpolate(contentType.nameTemplate)
                : undefined;
        });

        $scope.editIconTitle = '';
        $scope.moveIconTitle = '';
        $scope.deleteIconTitle = '';

        // localize the edit icon title
        localizationService.localize('general_edit').then(function (value) {
            $scope.editIconTitle = value;
        });

        // localize the delete icon title
        localizationService.localize('general_delete').then(function (value) {
            $scope.deleteIconTitle = value;
        });

        // localize the move icon title
        localizationService.localize('actions_move').then(function (value) {
            $scope.moveIconTitle = value;
        });

        $scope.nodes = [];
        $scope.currentNode = undefined;
        $scope.realCurrentNode = undefined;
        $scope.scaffolds = undefined;
        $scope.sorting = false;

        $scope.minItems = $scope.model.config.minItems || 0;
        $scope.maxItems = $scope.model.config.maxItems || 0;

        if ($scope.maxItems == 0)
            $scope.maxItems = 1000;

        $scope.singleMode = $scope.minItems == 1 && $scope.maxItems == 1;
        $scope.showIcons = $scope.model.config.showIcons || true;
        $scope.wideMode = $scope.model.config.hideLabel == "1";

        $scope.overlayMenu = {
            show: false,
            style: {}
        };

        $scope.addNode = function (alias) {
            var scaffold = $scope.getScaffold(alias);

            var newNode = initNode(scaffold, null);

            $scope.currentNode = newNode;

            $scope.closeNodeTypePicker();
        };

        $scope.openNodeTypePicker = function (event) {
            if ($scope.nodes.length >= $scope.maxItems) {
                return;
            }

            // this could be used for future limiting on node types
            $scope.overlayMenu.scaffolds = [];
            _.each($scope.scaffolds, function (scaffold) {
                var icon = scaffold.icon;
                // workaround for when no icon is chosen for a doctype
                if (icon == ".sprTreeFolder") {
                    icon = "icon-folder";
                }
                $scope.overlayMenu.scaffolds.push({
                    alias: scaffold.contentTypeAlias,
                    name: scaffold.contentTypeName,
                    icon: icon
                });
            });

            if ($scope.overlayMenu.scaffolds.length == 0) {
                return;
            }

            if ($scope.overlayMenu.scaffolds.length == 1) {
                // only one scaffold type - no need to display the picker
                $scope.addNode($scope.scaffolds[0].contentTypeAlias);
                return;
            }

            // calculate overlay position
            // - yeah... it's jQuery (ungh!) but that's how the Grid does it.
            var offset = $(event.target).offset();
            var scrollTop = $(event.target).closest(".umb-panel-body").scrollTop();
            if (offset.top < 400) {
                $scope.overlayMenu.style.top = 300 + scrollTop;
            }
            else {
                $scope.overlayMenu.style.top = offset.top - 150 + scrollTop;
            }
            $scope.overlayMenu.show = true;
        };

        $scope.closeNodeTypePicker = function () {
            $scope.overlayMenu.show = false;
        };

        $scope.editNode = function (idx) {
            if ($scope.currentNode && $scope.currentNode.id == $scope.nodes[idx].id) {
                $scope.currentNode = undefined;
            } else {
                $scope.currentNode = $scope.nodes[idx];
            }
        };

        $scope.deleteNode = function (idx) {
            if ($scope.nodes.length > $scope.model.config.minItems) {
                if ($scope.model.config.confirmDeletes && $scope.model.config.confirmDeletes == 1) {
                    if (confirm("Are you sure you want to delete this item?")) {
                        $scope.nodes.splice(idx, 1);
                        updateModel();
                    }
                } else {
                    $scope.nodes.splice(idx, 1);
                    updateModel();
                }
            }
        };

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
            return scaffold && scaffold.icon && scaffold.icon !== ".sprTreeFolder" ? scaffold.icon : "icon-folder";
        }

        $scope.sortableOptions = {
            axis: 'y',
            cursor: "move",
            handle: ".nested-content__icon--move",
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
                    $scope.sorting = false;
                    updateModel();
                });
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
            if ($scope.model.config.contentTypes.length == scaffoldsLoaded) {
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

            node.id = guid();
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
                $scope.$broadcast("ncSyncVal", { id: $scope.realCurrentNode.id });
            }
            if (inited) {
                var newValues = [];
                for (var i = 0; i < $scope.nodes.length; i++) {
                    var node = $scope.nodes[i];
                    var newValue = {
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

        var guid = function () {
            function _p8(s) {
                var p = (Math.random().toString(16) + "000000000").substr(2, 8);
                return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
            }
            return _p8() + _p8(true) + _p8(true) + _p8();
        };
    }

]);