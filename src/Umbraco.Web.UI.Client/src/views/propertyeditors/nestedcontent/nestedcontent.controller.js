(function () {
    'use strict';

    /**
     * When performing a copy, we do copy the ElementType Data Model, but each inner Nested Content property is still stored as the Nested Content Model, aka. each property is just storing its value. To handle this we need to ensure we handle both scenarios.
     */


    angular.module('umbraco').run(['clipboardService', function (clipboardService) {

        function resolveNestedContentPropertiesForPaste(prop, propClearingMethod) {

            // if prop.editor is "Umbraco.NestedContent"
            if ((typeof prop === 'object' && prop.editor === "Umbraco.NestedContent")) {

                var value = prop.value;
                for (var i = 0; i < value.length; i++) {
                    var obj = value[i];

                    // generate a new key.
                    obj.key = String.CreateGuid();

                    // Loop through all inner properties:
                    for (var k in obj) {
                        propClearingMethod(obj[k], clipboardService.TYPES.RAW);
                    }
                }
            }
        }

        clipboardService.registerPastePropertyResolver(resolveNestedContentPropertiesForPaste, clipboardService.TYPES.ELEMENT_TYPE)


        function resolveInnerNestedContentPropertiesForPaste(prop, propClearingMethod) {

            // if we got an array, and it has a entry with ncContentTypeAlias this meants that we are dealing with a NestedContent property data.
            if ((Array.isArray(prop) && prop.length > 0 && prop[0].ncContentTypeAlias !== undefined)) {

                for (var i = 0; i < prop.length; i++) {
                    var obj = prop[i];

                    // generate a new key.
                    obj.key = String.CreateGuid();

                    // Loop through all inner properties:
                    for (var k in obj) {
                        propClearingMethod(obj[k], clipboardService.TYPES.RAW);
                    }
                }
            }
        }

        clipboardService.registerPastePropertyResolver(resolveInnerNestedContentPropertiesForPaste, clipboardService.TYPES.RAW)
    }]);

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

    function NestedContentController($scope, $interpolate, $filter, serverValidationManager, contentResource, localizationService, iconHelper, clipboardService, eventsService, overlayService, $attrs) {

        const vm = this;
        
        var model = $scope.$parent.$parent.model;

        vm.readonly = false;

        var contentTypeAliases = [];
        _.each(model.config.contentTypes, function (contentType) {
            contentTypeAliases.push(contentType.ncAlias);
        });

        _.each(model.config.contentTypes, function (contentType) {
            contentType.nameExp = !!contentType.nameTemplate
                ? $interpolate(contentType.nameTemplate)
                : undefined;
        });

        $attrs.$observe('readonly', value => {
            vm.readonly = value !== undefined;

            vm.allowRemove = !vm.readonly;
            vm.allowAdd = !vm.readonly;

            vm.sortableOptions.disabled = vm.readonly;

            removeAllEntriesAction.isDisabled = vm.readonly;
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

        vm.singleMode = vm.minItems === 1 && vm.maxItems === 1 && model.config.contentTypes.length === 1;
        vm.expandsOnLoad = Object.toBoolean(model.config.expandsOnLoad)
        vm.showIcons = Object.toBoolean(model.config.showIcons);
        vm.wideMode = Object.toBoolean(model.config.hideLabel);
        vm.hasContentTypes = model.config.contentTypes.length > 0;

        var cultureChanged = eventsService.on('editors.content.cultureChanged', (name, args) => updateModel());

        var labels = {};
        vm.labels = labels;
        localizationService.localizeMany(["grid_addElement", "content_createEmpty", "actions_copy"]).then(function (data) {
            labels.grid_addElement = data[0];
            labels.content_createEmpty = data[1];
            labels.copy_icon_title = data[2];
        });

        function setCurrentNode(node, focusNode) {
            updateModel();
            vm.currentNode = node;
            vm.focusOnNode = focusNode;
        }

        var copyAllEntries = function () {

            syncCurrentNode();

            // list aliases
            var aliases = vm.nodes.map((node) => node.contentTypeAlias);

            // remove dublicates
            aliases = aliases.filter((item, index) => aliases.indexOf(item) === index);

            var nodeName = "";

            if (vm.umbVariantContent) {
                nodeName = vm.umbVariantContent.editor.content.name;
            }

            localizationService.localize("clipboard_labelForArrayOfItemsFrom", [model.label, nodeName]).then(function (data) {
                clipboardService.copyArray(clipboardService.TYPES.ELEMENT_TYPE, aliases, vm.nodes, data, "icon-thumbnail-list", model.id, clearNodeForCopy);
            });
        }

        let copyAllEntriesAction = {
            labelKey: "clipboard_labelForCopyAllEntries",
            labelTokens: [model.label],
            icon: "icon-documents",
            method: copyAllEntries,
            isDisabled: true,
            useLegacyIcon: false
        };

        let removeAllEntriesAction = {
            labelKey: "clipboard_labelForRemoveAllEntries",
            labelTokens: [],
            icon: "icon-trash",
            method: removeAllEntries,
            isDisabled: true,
            useLegacyIcon: false
        };
        
        function removeAllEntries() {

            localizationService.localizeMany(["content_nestedContentDeleteAllItems", "general_delete"]).then(data => {
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
        
        // helper to force the current form into the dirty state
        function setDirty() {
            if (vm.umbProperty) {
                vm.umbProperty.setDirty();
            }
        };

        function addNode(alias) {
            var scaffold = getScaffold(alias);

            var newNode = createNode(scaffold, null);

            setCurrentNode(newNode, true);
            setDirty();
            validate();
        };

        vm.openNodeTypePicker = function ($event) {

            if (vm.nodes.length >= vm.maxItems) {
                return;
            }

            var availableItems = [];
            _.each(vm.scaffolds, function (scaffold) {
                availableItems.push({
                    alias: scaffold.contentTypeAlias,
                    name: scaffold.contentTypeName,
                    icon: iconHelper.convertFromLegacyIcon(scaffold.icon),
                    tooltip: scaffold.documentType.description
                });
            });

            const dialog = {
                orderBy: "$index",
                view: "itempicker",
                event: $event,
                filter: availableItems.length > 12,
                size: availableItems.length > 6 ? "medium" : "small",
                availableItems: availableItems,
                clickPasteItem: function (item) {
                    if (Array.isArray(item.data)) {
                        _.each(item.data, function (entry) {
                            pasteFromClipboard(entry);
                        });
                    } else {
                        pasteFromClipboard(item.data);
                    }

                    overlayService.close();
                },
                submit: function (model) {
                    if (model && model.selectedItem) {
                        addNode(model.selectedItem.alias);
                    }

                    overlayService.close();
                },
                close: function () {
                    overlayService.close();
                }
            };

            if (dialog.availableItems.length === 0) {
                return;
            }

            dialog.pasteItems = [];

            var entriesForPaste = clipboardService.retrieveEntriesOfType(clipboardService.TYPES.ELEMENT_TYPE, contentTypeAliases);
            _.each(entriesForPaste, function (entry) {
                dialog.pasteItems.push({
                    date: entry.date,
                    name: entry.label,
                    data: entry.data,
                    icon: entry.icon
                });
            });

            dialog.pasteItems.sort( (a, b) => {
                return b.date - a.date
            });

            dialog.title = dialog.pasteItems.length > 0 ? labels.grid_addElement : labels.content_createEmpty;
            dialog.hideHeader = dialog.pasteItems.length > 0;

            dialog.clickClearPaste = function ($event) {
                $event.stopPropagation();
                $event.preventDefault();
                clipboardService.clearEntriesOfType(clipboardService.TYPES.ELEMENT_TYPE, contentTypeAliases);
                dialog.pasteItems = [];// This dialog is not connected via the clipboardService events, so we need to update manually.
                dialog.hideHeader = false;
            };

            if (dialog.availableItems.length === 1 && dialog.pasteItems.length === 0) {
                // only one scaffold type - no need to display the picker
                addNode(vm.scaffolds[0].contentTypeAlias);

                dialog.close();

                return;
            }

            overlayService.open(dialog);
        };

        vm.editNode = function (idx) {
            if (vm.currentNode && vm.currentNode.key === vm.nodes[idx].key) {
                setCurrentNode(null, false);
            } else {
                setCurrentNode(vm.nodes[idx], true);
            }
        };

        vm.canDeleteNode = function (idx) {
            return (vm.nodes.length > vm.minItems)
                ? true
                : model.config.contentTypes.length > 1;
        };

        function deleteNode(idx) {
            var removed = vm.nodes.splice(idx, 1);

            setDirty();

            removed.forEach(x => {
                // remove any server validation errors associated
                serverValidationManager.removePropertyError(x.key, vm.umbProperty.property.culture, vm.umbProperty.property.segment, "", { matchType: "contains" });
            });

            updateModel();
            validate();
        };

        vm.requestDeleteNode = function (idx) {
            if (!vm.canDeleteNode(idx)) {
                return;
            }
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
            if (!model.value || !model.value.length) {
                return "";
            }

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
                        if (newName && (newName = newName.trim())) {
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
            if (!model.value || !model.value.length) {
                return "";
            }

            var scaffold = getScaffold(model.value[idx].ncContentTypeAlias);
            return scaffold && scaffold.icon ? iconHelper.convertFromLegacyIcon(scaffold.icon) : "icon-folder";
        };

        vm.sortableOptions = {
            axis: "y",
            containment: "parent",
            cursor: "move",
            handle: '.umb-nested-content__header-bar',
            distance: 10,
            opacity: 0.7,
            tolerance: "pointer",
            scroll: true,
            disabled: vm.readOnly,
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

        function clearNodeForCopy(clonedData) {
            delete clonedData.key;
            delete clonedData.$$hashKey;

            var variant = clonedData.variants[0];
            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];
                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];

                    // If we have ncSpecific data, lets revert to standard data model.
                    if (prop.propertyAlias) {
                        prop.alias = prop.propertyAlias;
                        delete prop.propertyAlias;
                    }

                    if(prop.ncMandatory !== undefined) {
                        prop.validation.mandatory = prop.ncMandatory;
                        delete prop.ncMandatory;
                    }
                }
            }

        }

        vm.showCopy = clipboardService.isSupported();
        vm.showPaste = false;

        vm.clickCopy = function ($event, node) {

            syncCurrentNode();

            clipboardService.copy(clipboardService.TYPES.ELEMENT_TYPE, node.contentTypeAlias, node, null, null, null, clearNodeForCopy);
            $event.stopPropagation();
        }


        function pasteFromClipboard(newNode) {

            if (newNode === undefined) {
                return;
            }

            newNode = clipboardService.parseContentForPaste(newNode, clipboardService.TYPES.ELEMENT_TYPE);

            // generate a new key.
            newNode.key = String.CreateGuid();

            // Ensure we have NC data in place:
            var variant = newNode.variants[0];
            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];
                for (var p = 0; p < tab.properties.length; p++) {
                    extendPropertyWithNCData(tab.properties[p]);
                }
            }

            vm.nodes.push(newNode);
            setDirty();
            //updateModel();// done by setting current node...

            setCurrentNode(newNode, true);
        }

        function checkAbilityToPasteContent() {
            vm.showPaste = clipboardService.hasEntriesOfType(clipboardService.TYPES.ELEMENT_TYPE, contentTypeAliases);
        }

        var storageUpdate = eventsService.on("clipboardService.storageUpdate", checkAbilityToPasteContent);
        $scope.$on('$destroy', function () {
            storageUpdate();
        });
        var notSupported = [
            "Umbraco.Tags",
            "Umbraco.UploadField",
            "Umbraco.ImageCropper",
            "Umbraco.BlockList"
        ];

        // Initialize
        vm.scaffolds = [];

        contentResource.getScaffolds(-20, contentTypeAliases).then(function (scaffolds){
            // Loop through all the content types
            _.each(model.config.contentTypes, function (contentType){
                // Get the scaffold from the result
                var scaffold = scaffolds[contentType.ncAlias];

                // make sure it's an element type before allowing the user to create new ones
                if (scaffold.isElement) {
                    // remove all tabs except the specified tab
                    var tabs = scaffold.variants[0].tabs;
                    var tab = _.find(tabs, function (tab) {
                        return tab.id !== 0 && (tab.label.toLowerCase() === contentType.ncTabAlias.toLowerCase() || contentType.ncTabAlias === "");
                    });
                    scaffold.variants[0].tabs = [];
                    if (tab) {
                        scaffold.variants[0].tabs.push(tab);

                        tab.properties.forEach(
                            function (property) {
                                if (_.find(notSupported, function (x) { return x === property.editor; })) {
                                    property.notSupported = true;
                                    // TODO: Not supported message to be replaced with 'content_nestedContentEditorNotSupported' dictionary key. Currently not possible due to async/timing quirk.
                                    property.notSupportedMessage = "Property " + property.label + " uses editor " + property.editor + " which is not supported by Nested Content.";
                                }
                            }
                        );
                    }

                    // Ensure Culture Data for Complex Validation.
                    ensureCultureData(scaffold);

                    // Store the scaffold object
                    vm.scaffolds.push(scaffold);
                }
            });

            // Initialize once all scaffolds have been loaded
            initNestedContent();
        });

        /**
         * Ensure that the containing content variant language and current property culture is transferred along
         * to the scaffolded content object representing this block.
         * This is required for validation along with ensuring that the umb-property inheritance is constantly maintained.
         * @param {any} content
         */
         function ensureCultureData(content) {

            if (!content || !vm.umbVariantContent || !vm.umbProperty) return;

            if (vm.umbVariantContent.editor.content.language) {
                // set the scaffolded content's language to the language of the current editor
                content.language = vm.umbVariantContent.editor.content.language;
            }
            // currently we only ever deal with invariant content for blocks so there's only one
            content.variants[0].tabs.forEach(tab => {
                tab.properties.forEach(prop => {
                    // set the scaffolded property to the culture of the containing property
                    prop.culture = vm.umbProperty.property.culture;
                });
            });
        }

        var initNestedContent = function () {
            // Initialize when all scaffolds have loaded
            // Sort the scaffold explicitly according to the sort order defined by the data type.
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

            // Enforce min items if we only have one scaffold type
            var modelWasChanged = false;
            if (vm.nodes.length < vm.minItems && vm.scaffolds.length === 1) {
                for (var i = vm.nodes.length; i < model.config.minItems; i++) {
                    addNode(vm.scaffolds[0].contentTypeAlias);
                }
                modelWasChanged = true;
            }

            // If there is only one item and expandsOnLoad property is true, set it as current node
            if (vm.singleMode || (vm.expandsOnLoad && vm.nodes.length === 1)) {
                setCurrentNode(vm.nodes[0], false);
            }

            validate();

            vm.inited = true;

            if (modelWasChanged) {
                updateModel();
            }

            updatePropertyActionStates();
            checkAbilityToPasteContent();
        }

        function extendPropertyWithNCData(prop) {

            if (prop.propertyAlias === undefined) {
                // store the original alias before we change below, see notes
                prop.propertyAlias = prop.alias;

                // NOTE: This is super ugly, the reason it is like this is because it controls the label/html id in the umb-property component at a higher level.
                // not pretty :/ but we can't change this now since it would require a bunch of plumbing to be able to change the id's higher up.
                prop.alias = model.alias + "___" + prop.alias;
            }

            // TODO: Do we need to deal with this separately?
            // Force validation to occur server side as this is the
            // only way we can have consistency between mandatory and
            // regex validation messages. Not ideal, but it works.
            if(prop.ncMandatory === undefined) {
                prop.ncMandatory = prop.validation.mandatory;
                prop.validation = {
                    mandatory: false,
                    pattern: ""
                };
            }
        }

        function createNode(scaffold, fromNcEntry) {
            var node = Utilities.copy(scaffold);

            node.key = fromNcEntry && fromNcEntry.key ? fromNcEntry.key : String.CreateGuid();

            var variant = node.variants[0];

            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];

                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];

                    extendPropertyWithNCData(prop);

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
            copyAllEntriesAction.isDisabled = !model.value || !model.value.length;
            removeAllEntriesAction.isDisabled = copyAllEntriesAction.isDisabled || vm.readonly;
        }

        var propertyActions = [
            copyAllEntriesAction,
            removeAllEntriesAction
        ];

        this.$onInit = function () {
            if (vm.umbProperty) {
                vm.umbProperty.setPropertyActions(propertyActions);
            }
        };

        var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
            updateModel();
        });

        var validate = function () {
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

        var watcher = $scope.$watch(
            function () {
                return vm.nodes.length;
            },
            function () {
                validate();
            }
        );

        $scope.$on("$destroy", function () {
            unsubscribe();
            cultureChanged();
            watcher();
        });

    }

})();
