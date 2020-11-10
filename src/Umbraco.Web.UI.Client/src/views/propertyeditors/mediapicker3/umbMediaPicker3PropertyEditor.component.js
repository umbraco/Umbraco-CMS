(function () {
    "use strict";


    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbMediaPicker3PropertyEditor
     * @function
     *
     * @description
     * The component for the Media Picker property editor.
     */
    angular
        .module("umbraco")
        .component("umbMediaPicker3PropertyEditor", {
            templateUrl: "views/propertyeditors/MediaPicker3/umb-media-picker3-property-editor.html",
            controller: MediaPicker3Controller,
            controllerAs: "vm",
            bindings: {
                model: "="
            },
            require: {
                propertyForm: "^form",
                umbProperty: "?^umbProperty",
                umbVariantContent: '?^^umbVariantContent',
                umbVariantContentEditors: '?^^umbVariantContentEditors',
                umbElementEditorContent: '?^^umbElementEditorContent'
            }
        });

    function MediaPicker3Controller($scope, editorService, clipboardService, localizationService, overlayService, userService) {

        var unsubscribe = [];

        // Property actions:
        var copyAllBlocksAction = null;
        var deleteAllBlocksAction = null;

        var vm = this;

        vm.loading = true;
        vm.currentMediaInFocus = null;
        vm.setMediaFocus = function (media) {
            if (vm.currentMediaInFocus !== null) {
                vm.currentMediaInFocus.focus = false;
            }
            vm.currentMediaInFocus = media;
            media.focus = true;
        };

        vm.supportCopy = clipboardService.isSupported();


        vm.labels = {};

        localizationService.localizeMany(["grid_addElement", "content_createEmpty"]).then(function (data) {
            vm.labels.grid_addElement = data[0];
            vm.labels.content_createEmpty = data[1];
        });

        vm.$onInit = function() {

            vm.singleMode = vm.model.config.singleMode || false;
            vm.validationLimit = vm.model.config.validationLimit;

            if(typeof vm.model.value !== 'array' || vm.model.value === null) {
                vm.model.value = [];
            }

            copyAllBlocksAction = {
                labelKey: "clipboard_labelForCopyAllEntries",
                labelTokens: [vm.model.label],
                icon: "documents",
                method: requestCopyAllMedias,
                isDisabled: true
            };

            deleteAllBlocksAction = {
                labelKey: 'clipboard_labelForRemoveAllEntries',
                labelTokens: [],
                icon: 'trash',
                method: requestDeleteAllMedia,
                isDisabled: true
            };

            var propertyActions = [
                copyAllBlocksAction,
                deleteAllBlocksAction
            ];

            if (vm.umbProperty) {
                vm.umbProperty.setPropertyActions(propertyActions);
            }

            userService.getCurrentUser().then(function (userData) {

                if (!vm.model.config.startNodeId) {
                    if (vm.model.config.ignoreUserStartNodes === true) {
                        vm.model.config.startNodeId = -1;
                        vm.model.config.startNodeIsVirtual = true;
                    } else {
                        vm.model.config.startNodeId = userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0];
                        vm.model.config.startNodeIsVirtual = userData.startMediaIds.length !== 1;
                    }
                }

                // only allow users to add and edit media if they have access to the media section
                var hasAccessToMedia = userData.allowedSections.indexOf("media") !== -1;
                vm.allowEdit = hasAccessToMedia;
                vm.allowAdd = hasAccessToMedia;

                vm.loading = false;
            });

        };

        function setDirty() {
            if (vm.propertyForm) {
                vm.propertyForm.$setDirty();
            }
        }

        vm.add = add;
        function add() {
            var mediaPicker = {
                startNodeId: vm.model.config.startNodeId,
                startNodeIsVirtual: vm.model.config.startNodeIsVirtual,
                //dataTypeKey: vm.model.dataTypeKey,
                multiPicker: vm.isSingleMode !== true,
                submit: function (model) {
                    editorService.close();
                    setDirty();
                },
                close: function (model) {
                    editorService.close();
                    reloadUpdatedMediaItems(model.updatedMediaNodes);
                }
            }

            editorService.mediaPicker(mediaPicker);
        }


        function deleteAllMedias() {
            vm.model.value = [];
        }

        function activateBlock(mediaObject) {
            mediaObject.active = true;
        }

        function editBlock(mediaObject, mediaIndex, parentForm) {

            options = options || {};

            // this must be set
            if (mediaIndex === undefined) {
                throw "mediaIndex was not specified on call to editBlock";
            }

            var wasNotActiveBefore = mediaObject.active !== true;
            activateMedia(mediaObject);

            // make a clone to avoid editing model directly.
            var mediaObjectClone = Utilities.copy(mediaObject);

            var blockEditorModel = {
                $parentScope: $scope, // pass in a $parentScope, this maintains the scope inheritance in infinite editing
                $parentForm: parentForm || vm.propertyForm, // pass in a $parentForm, this maintains the FormController hierarchy with the infinite editing view (if it contains a form)
                createFlow: options.createFlow === true,
                title: mediaObjectClone.label,
                view: "views/common/infiniteeditors/MediaPicker3/mediapicker.html",
                submit: function(blockEditorModel) {
                    mediaObject.active = false;
                    editorService.close();
                },
                close: function(model) {
                    if (model.createFlow) {
                        deleteBlock(mediaObject);
                    } else {
                        if (wasNotActiveBefore === true) {
                            mediaObject.active = false;
                        }
                    }
                    editorService.close();
                }
            };

            // open property settings editor
            editorService.open(blockEditorModel);
        }

        vm.showAddDialog = showAddDialog;
        function showAddDialog(createIndex, $event) {
            console.log("shoa add dialog")
        };

        var requestCopyAllMedias = function() {
            // TODO..
        }
        function copyBlock(block) {
            //clipboardService.copy(clipboardService.TYPES.BLOCK, block.content.contentTypeAlias, {"layout": block.layout, "data": block.data, "settingsData":block.settingsData}, block.label, block.content.icon, block.content.udi);
        }
        function requestPasteFromClipboard(index, pasteEntry, pasteType) {

            if (pasteEntry === undefined) {
                return false;
            }

            //TODO...

            return true;

        }

        function requestDeleteBlock(block) {
            localizationService.localizeMany(["general_delete", "blockEditor_confirmDeleteBlockMessage", "contentTypeEditor_yesDelete"]).then(function (data) {
                const overlay = {
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [block.label]),
                    submitButtonLabel: data[2],
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        deleteBlock(block);
                        overlayService.close();
                    }
                };

                overlayService.confirmDelete(overlay);
            });
        }
        function requestDeleteAllMedia() {
            localizationService.localizeMany(["content_nestedContentDeleteAllItems", "general_delete"]).then(function (data) {
                overlayService.confirmDelete({
                    title: data[1],
                    content: data[0],
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        deleteAllBlocks();
                        overlayService.close();
                    }
                });
            });
        }


        vm.sortableOptions = {
            axis: "y",
            containment: "parent",
            cursor: "grabbing",
            handle: "umb-media-card",
            cancel: "input,textarea,select,option",
            classes: ".umb-media-card--dragging",
            distance: 5,
            tolerance: "pointer",
            scroll: true,
            update: function (ev, ui) {
                setDirty();
            }
        };


        function onAmountOfMediaChanged() {

            // enable/disable property actions
            if (copyAllBlocksAction) {
                copyAllBlocksAction.isDisabled = vm.model.value.length === 0;
            }
            if (deleteAllBlocksAction) {
                deleteAllBlocksAction.isDisabled = vm.model.value.length === 0;
            }

            // validate limits:
            if (vm.propertyForm && vm.validationLimit) {

                var isMinRequirementGood = vm.validationLimit.min === null || vm.model.value.length >= vm.validationLimit.min;
                vm.propertyForm.minCount.$setValidity("minCount", isMinRequirementGood);

                var isMaxRequirementGood = vm.validationLimit.max === null || vm.model.value.length <= vm.validationLimit.max;
                vm.propertyForm.maxCount.$setValidity("maxCount", isMaxRequirementGood);
            }
        }

        unsubscribe.push($scope.$watch(() => vm.model.value.length, onAmountOfMediaChanged));

        $scope.$on("$destroy", function () {
            for (const subscription of unsubscribe) {
                subscription();
            }
        });
    }

})();
