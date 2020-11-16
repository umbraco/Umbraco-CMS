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

        localizationService.localizeMany(["grid_addElement", "content_createEmpty", "mediaPicker_editMediaEntryLabel"]).then(function (data) {
            vm.labels.grid_addElement = data[0];
            vm.labels.content_createEmpty = data[1];
            vm.labels.mediaPicker_editMediaEntryLabel = data[2];
        });

        vm.$onInit = function() {

            console.log("property model: ", vm.model)

            vm.singleMode = vm.model.config.singleMode || false;
            vm.validationLimit = vm.model.config.validationLimit;

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

            if(vm.model.value === null || !Array.isArray(vm.model.value)) {
                vm.model.value = [];
            }

            vm.model.value.forEach(mediaEntry => updateMediaEntryData(mediaEntry));

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

                //editMedia(vm.model.value[0]);
            });

        };

        function setDirty() {
            if (vm.propertyForm) {
                vm.propertyForm.$setDirty();
            }
        }

        vm.addMediaAt = addMediaAt;
        function addMediaAt(createIndex, $event) {
            var mediaPicker = {
                startNodeId: vm.model.config.startNodeId,
                startNodeIsVirtual: vm.model.config.startNodeIsVirtual,
                //dataTypeKey: vm.model.dataTypeKey,
                multiPicker: vm.isSingleMode !== true,
                submit: function (model) {
                    editorService.close();

                    var indexIncrementor = 0;
                    model.selection.forEach((entry) => {
                        var mediaEntry = {};
                        mediaEntry.key = String.CreateGuid();
                        mediaEntry.mediaKey = entry.key;
                        updateMediaEntryData(mediaEntry);
                        vm.model.value.splice(createIndex + indexIncrementor, 0, mediaEntry);
                        indexIncrementor++;
                    });

                    setDirty();
                },
                close: function () {
                    editorService.close();
                }
            }

            editorService.mediaPicker(mediaPicker);
        }

        function updateMediaEntryData(mediaEntry) {

            mediaEntry.crops = mediaEntry.crops || [];
            mediaEntry.focalPoint = mediaEntry.focalPoint || {
                left: 0.5,
                top: 0.5
            };;

            // Copy config and only transfer coordinates.
            var newCrops = Utilities.copy(vm.model.config.crops);
            newCrops.forEach(crop => {
                console.log("mediaEntry.crops", mediaEntry.crops);
                var oldCrop = mediaEntry.crops.filter(x => x.alias === crop.alias).shift();
                console.log("oldCrop", oldCrop);
                if (oldCrop && oldCrop.height === crop.height && oldCrop.width === crop.width) {
                    crop.coordinates = oldCrop.coordinates;
                }
            });
            mediaEntry.crops = newCrops;

        }


        function deleteAllMedias() {
            vm.model.value = [];
        }

        vm.activeMediaEntry = null;
        function setActiveMedia(mediaEntryOrNull) {
            vm.activeMediaEntry = mediaEntryOrNull;
        }

        vm.editMedia = editMedia;
        function editMedia(mediaEntry, options) {

            options = options || {};

            setActiveMedia(mediaEntry);

            // make a clone to avoid editing model directly.
            var mediaEntryClone = Utilities.copy(mediaEntry);

            var mediaEditorModel = {
                $parentScope: $scope, // pass in a $parentScope, this maintains the scope inheritance in infinite editing
                $parentForm: vm.propertyForm, // pass in a $parentForm, this maintains the FormController hierarchy with the infinite editing view (if it contains a form)
                createFlow: options.createFlow === true,
                title: vm.labels.mediaPicker_editMediaEntryLabel,
                mediaEntry: mediaEntryClone,
                //cropsConfig: config.crops,
                //api: vm,
                view: "views/common/infiniteeditors/mediapicker3/mediaEntryEditor.html",
                size: "large",
                submit: function(model) {
                    vm.model.value[vm.model.value.indexOf(mediaEntry)] = mediaEntryClone;
                    setActiveMedia(null)
                    editorService.close();
                },
                close: function(model) {
                    if(model.createFlow === true) {
                        // This means that the user cancelled the creation and we should remove the media item.
                        // TODO: remove new media item.
                    }
                    setActiveMedia(null)
                    editorService.close();
                }
            };

            // open property settings editor
            editorService.open(mediaEditorModel);
        }

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

        function requestDeleteMedia(media) {
            localizationService.localizeMany(["general_delete", "media_confirmRemoveMediaEntryMessage", "general_remove"]).then(function (data) {
                const overlay = {
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [block.label]),
                    submitButtonLabel: data[2],
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        deleteMedia(media);
                        overlayService.close();
                    }
                };

                overlayService.confirmDelete(overlay);
            });
        }
        function requestDeleteAllMedia() {
            localizationService.localizeMany(["media_confirmRemoveAllMediaEntryMessage", "general_remove"]).then(function (data) {
                overlayService.confirmDelete({
                    title: data[1],
                    content: data[0],
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        deleteAllMedias();
                        overlayService.close();
                    }
                });
            });
        }


        vm.sortableOptions = {
            //containment: "parent",
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
