/**
 * @ngdoc controller
 * @name Umbraco.Editors.BlockList.BlockConfigurationOverlayController
 * @function
 *
 * @description
 * The controller for the content type editor property settings dialog
 */

(function () {
    "use strict";

    function BlockConfigurationOverlayController($scope, overlayService, localizationService, editorService, elementTypeResource, eventsService, udiService, angularHelper) {

        var unsubscribe = [];

        var vm = this;
        vm.block = $scope.model.block;

        vm.colorPickerOptions = {
            type: "color",
            allowEmpty: true,
            showAlpha: true
        };

        loadElementTypes();

        function loadElementTypes() {
            return elementTypeResource.getAll().then(function(elementTypes) {
                vm.elementTypes = elementTypes;

                vm.contentPreview = vm.getElementTypeByKey(vm.block.contentElementTypeKey);
                vm.settingsPreview = vm.getElementTypeByKey(vm.block.settingsElementTypeKey);
            });
        }

        vm.getElementTypeByKey = function(key) {
            return vm.elementTypes.find(function (type) {
                return type.key === key;
            });
        };

        vm.openElementType = function(elementTypeKey) {
            var elementType = vm.getElementTypeByKey(elementTypeKey);
            if (elementType) {
                var elementTypeId = elementType.id;
                const editor = {
                    id: elementTypeId,
                    submit: function (model) {
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.documentTypeEditor(editor);
            }
        };

        vm.createElementTypeAndCallback = function(callback) {
            const editor = {
                create: true,
                infiniteMode: true,
                isElement: true,
                noTemplate: true,
                submit: function (model) {
                    callback(model.documentTypeKey);
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.documentTypeEditor(editor);
        };

        vm.addSettingsForBlock = function($event, block) {

            localizationService.localize("blockEditor_headlineAddSettingsElementType").then(localizedTitle => {

                const settingsTypePicker = {
                    title: localizedTitle,
                    entityType: "documentType",
                    isDialog: true,
                    filter: node => {
                        if (node.metaData.isElement === true) {
                            return false;
                        }
                        return true;
                    },
                    filterCssClass: "not-allowed",
                    select: node => {
                        vm.applySettingsToBlock(block, udiService.getKey(node.udi));
                        editorService.close();
                    },
                    close: () => editorService.close(),
                    extraActions: [
                        {
                            style: "primary",
                            labelKey: "blockEditor_labelcreateNewElementType",
                            action: () => {
                                vm.createElementTypeAndCallback((key) => {
                                    vm.applySettingsToBlock(block, key);

                                    // At this point we will close the contentTypePicker.
                                    editorService.close();
                                });
                            }
                        }
                    ]
                };

                editorService.contentTypePicker(settingsTypePicker);

            });
        };

        vm.applySettingsToBlock = function(block, key) {
            block.settingsElementTypeKey = key;
            vm.settingsPreview = vm.getElementTypeByKey(vm.block.settingsElementTypeKey);
        };

        vm.requestRemoveSettingsForBlock = function(block) {
            localizationService.localizeMany(["general_remove", "defaultdialogs_confirmremoveusageof"]).then(function (data) {

                var settingsElementType = vm.getElementTypeByKey(block.settingsElementTypeKey);

                overlayService.confirmRemove({
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [(settingsElementType ? settingsElementType.name : "(Unavailable ElementType)")]),
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.removeSettingsForBlock(block);
                        overlayService.close();
                    }
                });
            });
        };

        vm.removeSettingsForBlock = function(block) {
            block.settingsElementTypeKey = null;
        };

        function updateUsedElementTypes(event, args) {
            var key = args.documentType.key;
            for (var i = 0; i<vm.elementTypes.length; i++) {
                if (vm.elementTypes[i].key === key) {
                    vm.elementTypes[i] = args.documentType;
                }
            }
            if (vm.contentPreview && vm.contentPreview.key === key) {
                vm.contentPreview = args.documentType;
                $scope.$evalAsync();
            }
            if (vm.settingsPreview && vm.settingsPreview.key === key) {
                vm.settingsPreview = args.documentType;
                $scope.$evalAsync();
            }
        }

        unsubscribe.push(eventsService.on("editors.documentType.saved", updateUsedElementTypes));

        vm.addViewForBlock = function(block) {
            localizationService.localize("blockEditor_headlineAddCustomView").then(localizedTitle => {

                const filePicker = {
                    title: localizedTitle,
                    isDialog: true,
                    filter: i => {
                        return !(i.name.indexOf(".html") !== -1);
                    },
                    filterCssClass: "not-allowed",
                    select: node => {
                        const filepath = decodeURIComponent(node.id.replace(/\+/g, " "));
                        block.view = "~/" + filepath.replace("wwwroot/", "");
                        editorService.close();
                    },
                    close: () => editorService.close()
                };

                editorService.staticFilePicker(filePicker);

            });
        };

        vm.requestRemoveViewForBlock = function(block) {
            localizationService.localizeMany(["general_remove", "defaultdialogs_confirmremoveusageof"]).then(function (data) {
                overlayService.confirmRemove({
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [block.view]),
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.removeViewForBlock(block);
                        overlayService.close();
                    }
                });
            });
        };

        vm.removeViewForBlock = function(block) {
            block.view = null;
        };

        vm.addStylesheetForBlock = function(block) {
          localizationService.localize("blockEditor_headlineAddCustomStylesheet").then(localizedTitle => {

            const filePicker = {
              title: localizedTitle,
              isDialog: true,
              filter: i => {
                return !(i.name.indexOf(".css") !== -1);
              },
              filterCssClass: "not-allowed",
              select: node => {
                const filepath = decodeURIComponent(node.id.replace(/\+/g, " "));
                block.stylesheet = "~/" + filepath.replace("wwwroot/", "");
                editorService.close();
              },
              close: () => editorService.close()
            };

            editorService.staticFilePicker(filePicker);

          });
        };

        vm.requestRemoveStylesheetForBlock = function(block) {
            localizationService.localizeMany(["general_remove", "defaultdialogs_confirmremoveusageof"]).then(function (data) {
                overlayService.confirmRemove({
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [block.stylesheet]),
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.removeStylesheetForBlock(block);
                        overlayService.close();
                    }
                });
            });
        };

        vm.removeStylesheetForBlock = function(block) {
            block.stylesheet = null;
        };

        vm.addThumbnailForBlock = function(block) {

          localizationService.localize("blockEditor_headlineAddThumbnail").then(localizedTitle => {

                let allowedFileExtensions = ['jpg', 'jpeg', 'png', 'svg', 'webp', 'gif'];

                const thumbnailPicker = {
                    title: localizedTitle,
                    isDialog: true,
                    filter: i => {
                        let ext = i.name.substr((i.name.lastIndexOf('.') + 1));
                        return allowedFileExtensions.includes(ext) === false;
                    },
                    filterCssClass: "not-allowed",
                    select: file => {
                        const id = decodeURIComponent(file.id.replace(/\+/g, " "));
                        block.thumbnail = "~/" + id.replace("wwwroot/", "");
                        editorService.close();
                    },
                    close: () => editorService.close()
                };

                editorService.staticFilePicker(thumbnailPicker);

            });
        };

        vm.removeThumbnailForBlock = function(entry) {
            entry.thumbnail = null;
        };

        vm.changeIconColor = function (color) {
            angularHelper.safeApply($scope, function () {
                vm.block.iconColor = color ? color.toString() : null;
            });
        };

        vm.changeBackgroundColor = function (color) {
            angularHelper.safeApply($scope, function () {
                vm.block.backgroundColor = color ? color.toString() : null;
            });
        };

        vm.submit = function() {
            if ($scope.model && $scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        };

        vm.close = function() {
            if ($scope.model && $scope.model.close) {
                $scope.model.close($scope.model);
            }
        };

        $scope.$on('$destroy', function() {
            unsubscribe.forEach(u => { u(); });
        });

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockList.BlockConfigurationOverlayController", BlockConfigurationOverlayController);

})();
