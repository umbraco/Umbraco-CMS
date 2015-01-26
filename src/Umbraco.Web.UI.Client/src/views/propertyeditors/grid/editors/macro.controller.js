angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.MacroController",
    function ($scope, $rootScope, $timeout, dialogService, macroResource, macroService,  $routeParams) {

        $scope.title = "Click to insert macro";

        $scope.setMacro = function(){
            dialogService.macroPicker({
                dialogData: {
                    richTextEditor: true,  
                    macroData: $scope.control.value || {
                        macroAlias: $scope.control.editor.config && $scope.control.editor.config.macroAlias
                          ? $scope.control.editor.config.macroAlias : ""
                    }
                },
                callback: function (data) {
                    $scope.control.value = {
                            macroAlias: data.macroAlias,
                            macroParamsDictionary: data.macroParamsDictionary
                    };

                    $scope.setPreview($scope.control.value );
                }
            });
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

