angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.MacroController",
        function ($scope, $timeout, editorService, macroResource, macroService, localizationService, $routeParams) {

            $scope.control.icon = $scope.control.icon || 'icon-settings-alt';

            localizationService.localize("grid_clickToInsertMacro").then(function(label) {
                $scope.title = label;
            });

            $scope.setMacro = function () {

                var dialogData = {
                    richTextEditor: true,
                    macroData: $scope.control.value || {
                        macroAlias: $scope.control.editor.config && $scope.control.editor.config.macroAlias
                            ? $scope.control.editor.config.macroAlias : ""
                    }
                };

                var macroPicker = {
                    dialogData: dialogData,
                    submit: function (model) {
                        var macroObject = macroService.collectValueData(model.selectedMacro, model.macroParams, dialogData.renderingEngine);

                        $scope.control.value = {
                            macroAlias: macroObject.macroAlias,
                            macroParamsDictionary: macroObject.macroParamsDictionary
                        };

                        $scope.setPreview($scope.control.value);
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                }
                editorService.macroPicker(macroPicker);
            };

            $scope.setPreview = function (macro) {
                var contentId = $routeParams.id;

                macroResource.getMacroResultAsHtmlForEditor(macro.macroAlias, contentId, macro.macroParamsDictionary)
                    .then(function (htmlResult) {
                        $scope.title = macro.macroAlias;
                        if (htmlResult.trim().length > 0 && htmlResult.indexOf("Macro:") < 0) {
                            $scope.preview = htmlResult;
                        }
                    });

            };

            $timeout(function () {
                if ($scope.control.$initializing) {
                    $scope.setMacro();
                } else if ($scope.control.value) {
                    $scope.setPreview($scope.control.value);
                }
            }, 200);
        });
