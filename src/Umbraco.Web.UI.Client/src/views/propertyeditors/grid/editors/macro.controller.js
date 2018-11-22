angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.MacroController",
    function ($scope, $rootScope, $timeout, dialogService, macroResource, macroService,  $routeParams) {

        $scope.title = "Click to insert macro";

        $scope.setMacro = function(){

            var dialogData = {
                richTextEditor: true,
                macroData: $scope.control.value || {
                    macroAlias: $scope.control.editor.config && $scope.control.editor.config.macroAlias
                      ? $scope.control.editor.config.macroAlias : ""
                }
            };

            $scope.macroPickerOverlay = {};
            $scope.macroPickerOverlay.view = "macropicker";
            $scope.macroPickerOverlay.dialogData = dialogData;
            $scope.macroPickerOverlay.show = true;

            $scope.macroPickerOverlay.submit = function(model) {

                var macroObject = macroService.collectValueData(model.selectedMacro, model.macroParams, dialogData.renderingEngine);

                $scope.control.value = {
                        macroAlias: macroObject.macroAlias,
                        macroParamsDictionary: macroObject.macroParamsDictionary
                };

                $scope.setPreview($scope.control.value );

                $scope.macroPickerOverlay.show = false;
                $scope.macroPickerOverlay = null;
            };

            $scope.macroPickerOverlay.close = function(oldModel) {
                $scope.macroPickerOverlay.show = false;
                $scope.macroPickerOverlay = null;
            };

    	};

        $scope.setPreview = function(macro){
            var contentId = $routeParams.id;

            macroResource.getMacroResultAsHtmlForEditor(macro.macroAlias, contentId, macro.macroParamsDictionary)
            .then(function (htmlResult) {
                $scope.title = macro.macroAlias;
                if(htmlResult.trim().length > 0 && htmlResult.indexOf("Macro:") < 0){
                    $scope.preview = htmlResult;
                }
            });

        };

    	$timeout(function(){
    		if($scope.control.$initializing){
    			$scope.setMacro();
    		}else if($scope.control.value){
                $scope.setPreview($scope.control.value);
            }
    	}, 200);
});
