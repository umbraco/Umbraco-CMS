angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.MacroController",
    function ($scope, $rootScope, $timeout, dialogService, macroResource, macroService,  $routeParams) {
    	
        $scope.title = "Click to insert macro";
        $scope.setMacro = function(){
    		dialogService.macroPicker({
                callback: function (data) {

                    $scope.control.value = {
                            syntax: data.syntax,
                            macroAlias: data.macroAlias,
                            marcoParamsDictionary: data.macroParamsDictionary
                    };
                        
                    $scope.setPreview(data);
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
    		if($scope.control.value === null){
    			$scope.setMacro();
    		}else{
                var parsed = macroService.parseMacroSyntax($scope.control.value.syntax);
                $scope.setPreview(parsed);
            }
    	}, 200);
});