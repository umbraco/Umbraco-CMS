(function () {
    'use strict';

    angular
        .module('umbraco')
        .component('nestedContentPropertyEditor', {
            templateUrl: 'views/propertyeditors/nestedcontent/nestedcontent.propertyeditor.html',
            controller: NestedContentController,
            controllerAs: 'vm',
            require: {
                umbProperty: '?^umbProperty',
                umbVariantContent: '?^^umbVariantContent'
            }
        });

    function NestedContentController($scope, $interpolate, $filter, $timeout, contentResource, localizationService, iconHelper, clipboardService, eventsService, overlayService, $routeParams, editorState) {
        
        var vm = this;
        var model = $scope.$parent.$parent.model;

        var contentTypeAliases = [];
        _.each(model.config.contentTypes, function (contentType) {
            contentTypeAliases.push(contentType.ncAlias);
        });

        _.each(model.config.contentTypes, function (contentType) {
            contentType.nameExp = !!contentType.nameTemplate
                ? $interpolate(contentType.nameTemplate)
                : undefined;
        });

        vm.nodes = [];
        vm.currentNode = null;
        vm.scaffolds = null;
        vm.sorting = false;
        vm.inited = false;

        vm.minItems = model.config.minItems || 0;
        vm.maxItems = model.config.maxItems || 0;

        if (vm.maxItems === 0)
            vm.maxItems = 1000;

        vm.singleMode = vm.minItems === 1 && vm.maxItems === 1;
        vm.showIcons = Object.toBoolean(model.config.showIcons);
        vm.wideMode = Object.toBoolean(model.config.hideLabel);
        vm.hasContentTypes = model.config.contentTypes.length > 0;

        var labels = {};
        vm.labels = labels;
        localizationService.localizeMany(["grid_addElement", "content_createEmpty", "actions_copy"]).then(function (data) {
            labels.grid_addElement = data[0];
            labels.content_createEmpty = data[1];
            labels.copy_icon_title = data[2]
        });

        function setCurrentNode(node) {
            updateModel();
            vm.currentNode = node;
        }
        
        var copyAllEntries = function() {
            
            syncCurrentNode();

            // list aliases
            var aliases = vm.nodes.map((node) => node.contentTypeAlias);

            // remove dublicates
            aliases = aliases.filter((item, index) => aliases.indexOf(item) === index);
            
            var nodeName = "";

            if(vm.umbVariantContent) {
                nodeName = vm.umbVariantContent.editor.content.name;
            }

            localizationService.localize("clipboard_labelForArrayOfItemsFrom", [model.label, nodeName]).then(function(data) {
                clipboardService.copyArray("elementTypeArray", aliases, vm.nodes, data, "icon-thumbnail-list", model.id);
            });
        }

        var copyAllEntriesAction = {
            labelKey: 'clipboard_labelForCopyAllEntries',
            labelTokens: [model.label],
            icon: 'documents',
            method: copyAllEntries,
            isDisabled: true
        }


        var removeAllEntries = function () {
            localizationService.localizeMany(["content_nestedContentDeleteAllItems", "general_delete"]).then(function (data) {
                overlayService.confirmDelete({
                    title: data[1],
                    content: data[0],
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.nodes = [];
                        setDirty();
                        updateModel();
                        overlayService.close();
                    }
                });
            });
        }

        var removeAllEntriesAction = {
            labelKey: 'clipboard_labelForRemoveAllEntries',
            labelTokens: [],
            icon: 'trash',
            method: removeAllEntries,
            isDisabled: true
        }



        // helper to force the current form into the dirty state
        function setDirty() {
            if ($scope.$parent.$parent.propertyForm) {
                $scope.$parent.$parent.propertyForm.$setDirty();
            }
        };

        function addNode(alias) {
            var scaffold = getScaffold(alias);

            var newNode = createNode(scaffold, null);

            setCurrentNode(newNode);
            setDirty();
        };

        vm.openNodeTypePicker = function ($event) {
            if (vm.overlayMenu || vm.nodes.length >= vm.maxItems) {
                return;
            }

            vm.overlayMenu = {
                show: false,
                style: {},
                filter: vm.scaffolds.length > 12 ? true : false,
                orderBy: "$index",
                view: "itempicker",
                event: $event,
                clickPasteItem: function(item) {
                    if (item.type === "elementTypeArray") {
                        _.each(item.data, function (entry) {
                            pasteFromClipboard(entry);
                        });
                    } else {
                        pasteFromClipboard(item.data);
                    }
                    vm.overlayMenu.show = false;
                    vm.overlayMenu = null;
                },
                submit: function (model) {
                    if (model && model.selectedItem) {
                        addNode(model.selectedItem.alias);
                    }
                    vm.overlayMenu.show = false;
                    vm.overlayMenu = null;
                },
                close: function () {
                    vm.overlayMenu.show = false;
                    vm.overlayMenu = null;
                }
            };

            // this could be used for future limiting on node types
            vm.overlayMenu.availableItems = [];
            _.each(vm.scaffolds, function (scaffold) {
                vm.overlayMenu.availableItems.push({
                    alias: scaffold.contentTypeAlias,
                    name: scaffold.contentTypeName,
                    icon: iconHelper.convertFromLegacyIcon(scaffold.icon)
                });
            });

            if (vm.overlayMenu.availableItems.length === 0) {
                return;
            }
            
            vm.overlayMenu.size = vm.overlayMenu.availableItems.length > 6 ? "medium" : "small";
            
            vm.overlayMenu.pasteItems = [];

            var singleEntriesForPaste = clipboardService.retriveEntriesOfType("elementType", contentTypeAliases);
            _.each(singleEntriesForPaste, function (entry) {
                vm.overlayMenu.pasteItems.push({
                    type: "elementType",
                    name: entry.label,
                    data: entry.data,
                    icon: entry.icon
                });
            });
            
            var arrayEntriesForPaste = clipboardService.retriveEntriesOfType("elementTypeArray", contentTypeAliases);
            _.each(arrayEntriesForPaste, function (entry) {
                vm.overlayMenu.pasteItems.push({
                    type: "elementTypeArray",
                    name: entry.label,
                    data: entry.data,
                    icon: entry.icon
                });
            });

            vm.overlayMenu.title = vm.overlayMenu.pasteItems.length > 0 ? labels.grid_addElement : labels.content_createEmpty;

            vm.overlayMenu.clickClearPaste = function ($event) {
                $event.stopPropagation();
                $event.preventDefault();
                clipboardService.clearEntriesOfType("elementType", contentTypeAliases);
                clipboardService.clearEntriesOfType("elementTypeArray", contentTypeAliases);
                vm.overlayMenu.pasteItems = [];// This dialog is not connected via the clipboardService events, so we need to update manually.
            };

            if (vm.overlayMenu.availableItems.length === 1 && vm.overlayMenu.pasteItems.length === 0) {
                // only one scaffold type - no need to display the picker
                addNode(vm.scaffolds[0].contentTypeAlias);
                return;
            }

            vm.overlayMenu.show = true;
        };

        vm.editNode = function (idx) {
            if (vm.currentNode && vm.currentNode.key === vm.nodes[idx].key) {
                setCurrentNode(null);
            } else {
                setCurrentNode(vm.nodes[idx]);
            }
        };

        function deleteNode(idx) {
            vm.nodes.splice(idx, 1);
            setDirty();
            updateModel();
        };
        vm.requestDeleteNode = function (idx) {
            if (model.config.confirmDeletes === true) {
                localizationService.localizeMany(["content_nestedContentDeleteItem", "general_delete", "general_cancel", "contentTypeEditor_yesDelete"]).then(function (data) {
                    const overlay = {
                        title: data[1],
                        content: data[0],
                        closeButtonLabel: data[2],
                        submitButtonLabel: data[3],
                        submitButtonStyle: "danger",
                        close: function () {
                            overlayService.close();
                        },
                        submit: function () {
                            deleteNode(idx);
                            overlayService.close();
                        }
                    };

                    overlayService.open(overlay);
                });
            } else {
                deleteNode(idx);
            }
        };

        vm.getName = function (idx) {

            var name = "";

            if (model.value[idx]) {

                var contentType = getContentTypeConfig(model.value[idx].ncContentTypeAlias);

                if (contentType != null) {
                    // first try getting a name using the configured label template
                    if (contentType.nameExp) {
                        // Run the expression against the stored dictionary value, NOT the node object
                        var item = model.value[idx];

                        // Add a temporary index property
                        item["$index"] = (idx + 1);

                        var newName = contentType.nameExp(item);
                        if (newName && (newName = $.trim(newName))) {
                            name = newName;
                        }

                        // Delete the index property as we don't want to persist it
                        delete item["$index"];
                    }

                    // if we still do not have a name and we have multiple content types to choose from, use the content type name (same as is shown in the content type picker)
                    if (!name && vm.scaffolds.length > 1) {
                        var scaffold = getScaffold(contentType.ncAlias);
                        if (scaffold) {
                            name = scaffold.contentTypeName;
                        }
                    }
                }

            }

            if (!name) {
                name = "Item " + (idx + 1);
            }

            // Update the nodes actual name value
            if (vm.nodes[idx].name !== name) {
                vm.nodes[idx].name = name;
            }

            return name;
        };

        vm.getIcon = function (idx) {
            var scaffold = getScaffold(model.value[idx].ncContentTypeAlias);
            return scaffold && scaffold.icon ? iconHelper.convertFromLegacyIcon(scaffold.icon) : "icon-folder";
        }

        vm.sortableOptions = {
            axis: "y",
            cursor: "move",
            handle: '.umb-nested-content__header-bar',
            distance: 10,
            opacity: 0.7,
            tolerance: "pointer",
            scroll: true,
            start: function (ev, ui) {
                updateModel();
                // Yea, yea, we shouldn't modify the dom, sue me
                $("#umb-nested-content--" + model.id + " .umb-rte textarea").each(function () {
                    tinymce.execCommand("mceRemoveEditor", false, $(this).attr("id"));
                    $(this).css("visibility", "hidden");
                });
                $scope.$apply(function () {
                    vm.sorting = true;
                });
            },
            update: function (ev, ui) {
                setDirty();
            },
            stop: function (ev, ui) {
                $("#umb-nested-content--" + model.id + " .umb-rte textarea").each(function () {
                    tinymce.execCommand("mceAddEditor", true, $(this).attr("id"));
                    $(this).css("visibility", "visible");
                });
                $scope.$apply(function () {
                    vm.sorting = false;
                    updateModel();
                });
            }
        };

        function getScaffold(alias) {
            return _.find(vm.scaffolds, function (scaffold) {
                return scaffold.contentTypeAlias === alias;
            });
        }

        function getContentTypeConfig(alias) {
            return _.find(model.config.contentTypes, function (contentType) {
                return contentType.ncAlias === alias;
            });
        }

        vm.showCopy = clipboardService.isSupported();
        vm.showPaste = false;

        vm.clickCopy = function ($event, node) {

            syncCurrentNode();

            clipboardService.copy("elementType", node.contentTypeAlias, node);
            $event.stopPropagation();
        }
        
        
        function pasteFromClipboard(newNode) {
            
            if (newNode === undefined) {
                return;
            }

            // generate a new key.
            newNode.key = String.CreateGuid();

            vm.nodes.push(newNode);
            setDirty();
            //updateModel();// done by setting current node...
            
            setCurrentNode(newNode);
        }

        function checkAbilityToPasteContent() {
            vm.showPaste = clipboardService.hasEntriesOfType("elementType", contentTypeAliases) || clipboardService.hasEntriesOfType("elementTypeArray", contentTypeAliases);
        }

        eventsService.on("clipboardService.storageUpdate", checkAbilityToPasteContent);

        var notSupported = [
            "Umbraco.Tags",
            "Umbraco.UploadField",
            "Umbraco.ImageCropper"
        ];

        // Initialize
        var scaffoldsLoaded = 0;
        vm.scaffolds = [];
        _.each(model.config.contentTypes, function (contentType) {
            contentResource.getScaffold(-20, contentType.ncAlias).then(function (scaffold) {
                // make sure it's an element type before allowing the user to create new ones
                if (scaffold.isElement) {
                    // remove all tabs except the specified tab
                    var tabs = scaffold.variants[0].tabs;
                    var tab = _.find(tabs, function (tab) {
                        return tab.id !== 0 && (tab.alias.toLowerCase() === contentType.ncTabAlias.toLowerCase() || contentType.ncTabAlias === "");
                    });
                    scaffold.variants[0].tabs = [];
                    if (tab) {
                        scaffold.variants[0].tabs.push(tab);

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
                    vm.scaffolds.push(scaffold);
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
            if (model.config.contentTypes.length === scaffoldsLoaded) {
                // Because we're loading the scaffolds async one at a time, we need to
                // sort them explicitly according to the sort order defined by the data type.
                contentTypeAliases = [];
                _.each(model.config.contentTypes, function (contentType) {
                    contentTypeAliases.push(contentType.ncAlias);
                });
                vm.scaffolds = $filter("orderBy")(vm.scaffolds, function (s) {
                    return contentTypeAliases.indexOf(s.contentTypeAlias);
                });

                // Convert stored nodes
                if (model.value) {
                    for (var i = 0; i < model.value.length; i++) {
                        var item = model.value[i];
                        var scaffold = getScaffold(item.ncContentTypeAlias);
                        if (scaffold == null) {
                            // No such scaffold - the content type might have been deleted. We need to skip it.
                            continue;
                        }
                        createNode(scaffold, item);
                    }
                }

                // Auto-fill with elementTypes, but only if we have one type to choose from, and if this property is empty.
                if (vm.singleMode === true && vm.nodes.length === 0 && model.config.minItems > 0) {
                    for (var i = vm.nodes.length; i < model.config.minItems; i++) {
                        addNode(vm.scaffolds[0].contentTypeAlias);
                    }
                }

                // If there is only one item, set it as current node
                if (vm.singleMode || (vm.nodes.length === 1 && vm.maxItems === 1)) {
                    setCurrentNode(vm.nodes[0]);
                }

                vm.inited = true;

                updatePropertyActionStates();
                checkAbilityToPasteContent();
            }
        }

        function createNode(scaffold, fromNcEntry) {
            var node = angular.copy(scaffold);

            node.key = fromNcEntry && fromNcEntry.key ? fromNcEntry.key : String.CreateGuid();

            var variant = node.variants[0];

            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];

                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];

                    prop.propertyAlias = prop.alias;
                    prop.alias = model.alias + "___" + prop.alias;
                    // Force validation to occur server side as this is the
                    // only way we can have consistency between mandatory and
                    // regex validation messages. Not ideal, but it works.
                    prop.ncMandatory = prop.validation.mandatory;
                    prop.validation = {
                        mandatory: false,
                        pattern: ""
                    };

                    if (fromNcEntry && fromNcEntry[prop.propertyAlias]) {
                        prop.value = fromNcEntry[prop.propertyAlias];
                    }
                }
            }

            vm.nodes.push(node);

            return node;
        }

        function convertNodeIntoNCEntry(node) {
            var obj = {
                key: node.key,
                name: node.name,
                ncContentTypeAlias: node.contentTypeAlias
            };
            for (var t = 0; t < node.variants[0].tabs.length; t++) {
                var tab = node.variants[0].tabs[t];
                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];
                    if (typeof prop.value !== "function") {
                        obj[prop.propertyAlias] = prop.value;
                    }
                }
            }
            return obj;
        }




        function syncCurrentNode() {
            if (vm.currentNode) {
                $scope.$broadcast("ncSyncVal", { key: vm.currentNode.key });
            }
        }

        function updateModel() {
            syncCurrentNode();

            if (vm.inited) {
                var newValues = [];
                for (var i = 0; i < vm.nodes.length; i++) {
                    newValues.push(convertNodeIntoNCEntry(vm.nodes[i]));
                }
                model.value = newValues;
            }

            updatePropertyActionStates();
        }

        function updatePropertyActionStates() {
            copyAllEntriesAction.isDisabled = !model.value || model.value.length === 0;
            removeAllEntriesAction.isDisabled = !model.value || model.value.length === 0;
        }


        
        var propertyActions = [
            copyAllEntriesAction,
            removeAllEntriesAction
        ];
        
        this.$onInit = function () {
            if (this.umbProperty) {
                this.umbProperty.setPropertyActions(propertyActions);
            }
        };

        var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
            updateModel();
        });

        var watcher = $scope.$watch(
            function () {
                return vm.nodes.length;
            },
            function () {
                //Validate!
                if (vm.nodes.length < vm.minItems) {
                    $scope.nestedContentForm.minCount.$setValidity("minCount", false);
                }
                else {
                    $scope.nestedContentForm.minCount.$setValidity("minCount", true);
                }

                if (vm.nodes.length > vm.maxItems) {
                    $scope.nestedContentForm.maxCount.$setValidity("maxCount", false);
                }
                else {
                    $scope.nestedContentForm.maxCount.$setValidity("maxCount", true);
                }
            }
        );

        $scope.$on("$destroy", function () {
            unsubscribe();
            watcher();
        });

    }

})();
