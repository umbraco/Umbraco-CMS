(function() {
    "use strict";

    function GridRichTextEditorController($scope, tinyMceService, macroService) {

        var vm = this;

        vm.openMediaPicker = openMediaPicker;
        vm.openMacroPicker = openMacroPicker;
        vm.openEmbed = openEmbed;

        function openMediaPicker(editor, currentTarget, userData) {
            vm.mediaPickerOverlay = {
                currentTarget: currentTarget,
                onlyImages: true,
                showDetails: true,
                startNodeId: userData.startMediaId,
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
