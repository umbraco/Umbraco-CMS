(function() {
    "use strict";

    function GridRichTextEditorController($scope, tinyMceService, macroService, editorState, entityResource) {

        var vm = this;

        vm.openLinkPicker = openLinkPicker;
        vm.openMediaPicker = openMediaPicker;
        vm.openMacroPicker = openMacroPicker;
        vm.openEmbed = openEmbed;

        function openLinkPicker(editor, currentTarget, anchorElement) {

            entityResource.getAnchors(JSON.stringify($scope.model.value)).then(function(anchorValues) {
                vm.linkPickerOverlay = {
                    view: "linkpicker",
                    currentTarget: currentTarget,
                    anchors: anchorValues,
                    dataTypeId: $scope.model.dataTypeId,
                    ignoreUserStartNodes : $scope.model.config.ignoreUserStartNodes,
                    show: true,
                    submit: function(model) {
                        tinyMceService.insertLinkInEditor(editor, model.target, anchorElement);
                        vm.linkPickerOverlay.show = false;
                        vm.linkPickerOverlay = null;
                    }
                };
            });

        }

        function openMediaPicker(editor, currentTarget, userData) {
            var startNodeId = userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0];
            var startNodeIsVirtual = userData.startMediaIds.length !== 1;


            if ($scope.model.config.ignoreUserStartNodes === "1") {
                startNodeId = -1;
                startNodeIsVirtual = true;
            }

            vm.mediaPickerOverlay = {
                currentTarget: currentTarget,
                onlyImages: true,
                showDetails: true,
                startNodeId: startNodeId,
                startNodeIsVirtual: startNodeIsVirtual,
                dataTypeId: $scope.model.dataTypeId,
                view: "mediapicker",
                show: true,
                submit: function(model) {
                    tinyMceService.insertMediaInEditor(editor, model.selectedImages[0]);
                    vm.mediaPickerOverlay.show = false;
                    vm.mediaPickerOverlay = null;
                }
            };
        }

        function openEmbed(editor) {
            vm.embedOverlay = {
                view: "embed",
                show: true,
                submit: function(model) {
                    tinyMceService.insertEmbeddedMediaInEditor(editor, model.embed.preview);
                    vm.embedOverlay.show = false;
                    vm.embedOverlay = null;
                }
            };
        }

        function openMacroPicker(editor, dialogData) {
            vm.macroPickerOverlay = {
                view: "macropicker",
                dialogData: dialogData,
                show: true,
                submit: function(model) {
                    var macroObject = macroService.collectValueData(model.selectedMacro, model.macroParams, dialogData.renderingEngine);
                    tinyMceService.insertMacroInEditor(editor, macroObject, $scope);
                    vm.macroPickerOverlay.show = false;
                    vm.macroPickerOverlay = null;
                }
            };
        }



    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.Grid.RichTextEditorController", GridRichTextEditorController);

})();
