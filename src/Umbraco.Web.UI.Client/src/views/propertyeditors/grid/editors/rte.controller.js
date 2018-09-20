(function() {
    "use strict";

    function GridRichTextEditorController($scope, tinyMceService, macroService, editorState, editorService) {

        var vm = this;

        vm.openLinkPicker = openLinkPicker;
        vm.openMediaPicker = openMediaPicker;
        vm.openMacroPicker = openMacroPicker;
        vm.openEmbed = openEmbed;

        function openLinkPicker(editor, currentTarget, anchorElement) {
            var linkPicker = {
                currentTarget: currentTarget,
				anchors: tinyMceService.getAnchorNames(JSON.stringify(editorState.current.properties)),
                submit: function(model) {
                    tinyMceService.insertLinkInEditor(editor, model.target, anchorElement);
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };
            editorService.linkPicker(linkPicker);
        }

        function openMediaPicker(editor, currentTarget, userData) {
            var mediaPicker = {
                currentTarget: currentTarget,
                onlyImages: true,
                showDetails: true,
                startNodeId: userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0],
                submit: function(model) {
                    tinyMceService.insertMediaInEditor(editor, model.selectedImages[0]);
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };
            editorService.mediaPicker(mediaPicker);
        }

        function openEmbed(editor) {
            var embed = {
                submit: function(model) {
                    tinyMceService.insertEmbeddedMediaInEditor(editor, model.embed.preview);
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };
            editorService.embed(embed);
        }

        function openMacroPicker(editor, dialogData) {
            var macroPicker = {
                dialogData: dialogData,
                submit: function(model) {
                    var macroObject = macroService.collectValueData(model.selectedMacro, model.macroParams, dialogData.renderingEngine);
                    tinyMceService.insertMacroInEditor(editor, macroObject, $scope);
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };
            editorService.macroPicker(macroPicker);
        }

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.Grid.RichTextEditorController", GridRichTextEditorController);

})();
