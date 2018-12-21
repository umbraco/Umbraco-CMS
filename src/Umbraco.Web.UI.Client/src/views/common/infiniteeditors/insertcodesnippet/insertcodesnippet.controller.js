(function () {
    "use strict";

    function InsertOverlayController($scope, localizationService, editorService) {

        var vm = this;

        vm.openMacroPicker = openMacroPicker;
        vm.openPageFieldOverlay = openPageFieldOverlay;
        vm.openDictionaryItemOverlay = openDictionaryItemOverlay;
        vm.openPartialOverlay = openPartialOverlay;
        vm.close = close;

        function onInit() {
            
            if(!$scope.model.title) {
                localizationService.localize("template_insert").then(function(value){
                    $scope.model.title = value;
                });
            }
    
            if(!$scope.model.subtitle) {
                localizationService.localize("template_insertDesc").then(function(value){
                    $scope.model.subtitle = value;
                });
            }
            
        }

        function openMacroPicker() {
            var macroPicker = {
                dialogData: {},
                submit: function(model) {
                    $scope.model.insert = {
                        "type": "macro",
                        "macroParams": model.macroParams,
                        "selectedMacro": model.selectedMacro
                    };
                    $scope.model.submit($scope.model);
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };
            editorService.macroPicker(macroPicker);
        }

        function openPageFieldOverlay() {
            var insertFieldEditor = {
                submit: function(model) {
                  $scope.model.insert = {
                      "type": "umbracoField",
                      "umbracoField": model.umbracoField
                  };
                  $scope.model.submit($scope.model);
                  editorService.close();
                },
                close: function (model) {
                    editorService.close();
                }
            };
            editorService.insertField(insertFieldEditor);
        }

        function openDictionaryItemOverlay() {

            var labelKeys = [
                "template_insertDictionaryItem", 
                "template_insertDictionaryItemDesc", 
                "emptyStates_emptyDictionaryTree"
            ];

            localizationService.localizeMany(labelKeys).then(function(values){
                var title = values[0];
                var subtitle = values[1];
                var emptyStateMessage = values[2];

                var dictionaryItemPicker = {
                    section: "translation",
                    treeAlias: "dictionary",
                    entityType: "dictionary",
                    multiPicker: false,
                    title: title,
                    subtitle: subtitle,
                    emptyStateMessage: emptyStateMessage,
                    select: function(node){
                        $scope.model.insert = {
                            "type": "dictionary",
                            "node": node
                        };
                        $scope.model.submit($scope.model);
                        editorService.close();
                    },
    
                    close: function() {
                        editorService.close();
                    }
                };

                editorService.treePicker(dictionaryItemPicker);

            });
        }

        function openPartialOverlay() {
            localizationService.localize("template_insertPartialView").then(function(value){
                var title = value;

                var partialItemPicker = {
                    section: "settings",
                    treeAlias: "partialViews",
                    entityType: "partialView",
                    multiPicker: false,
                    title: title,
                    filter: function(i) {
                        if(i.name.indexOf(".cshtml") === -1 && i.name.indexOf(".vbhtml") === -1) {
                            return true;
                        }
                    },
                    filterCssClass: "not-allowed",
                    select: function(node) {
                        $scope.model.insert = {
                            "type": "partial",
                            "node": node
                        };
                        $scope.model.submit($scope.model); 
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };

                editorService.treePicker(partialItemPicker);
            });
        }

        function close() {
            if($scope.model.close) {
                $scope.model.close();
            }
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.InsertOverlay", InsertOverlayController);
})();
