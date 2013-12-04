//DO NOT DELETE THIS, this is in use... 
angular.module('umbraco')
.controller("Umbraco.PropertyEditors.MacroContainerController",
	
	function($scope, dialogService, entityResource, macroService, macroResource){
		$scope.renderModel = [];
		
		if($scope.model.value){
			var macros = $scope.model.value.split('>');

			angular.forEach(macros, function(syntax, key){
				if(syntax && syntax.length > 10){
					//re-add the char we split on
					syntax = syntax + ">";
					var parsed = macroService.parseMacroSyntax(syntax);
					if(!parsed){
						parsed = {};
					}
					
					parsed.syntax = syntax;
					collectDetails(parsed);
					$scope.renderModel.push(parsed);
				}
			});
		}

		
		function collectDetails(macro){
			macro.details = "";
			if(macro.marcoParamsDictionary){
				angular.forEach((macro.marcoParamsDictionary), function(value, key){
					macro.details += key + ": " + value + " ";	
				});	
			}		
		}

		function openDialog(index){
			var dialogData = {};

			if(index){
				var macro = $scope.renderModel[index];
				dialogData = {macroData: macro};
			}
			
			dialogService.macroPicker({
                scope: $scope,
                dialogData : dialogData,
                    callback: function(data) {

                    	collectDetails(data);

                        //update the raw syntax and the list...
                        if(index){
                        	$scope.renderModel[index] = data;
                        }else{
                        	$scope.renderModel.push(data);
                        }
                    }
                });
		}	



		$scope.edit =function(index){
				openDialog(index);
		};

		$scope.add =function(){
				openDialog();
		};

		$scope.remove =function(index){
			$scope.renderModel.splice(index, 1);
			$scope.macros.splice(index, 1);
			$scope.model.value = trim($scope.macros.join(), ",");
		};

	    $scope.clear = function() {
	        $scope.model.value = "";
	        $scope.renderModel = [];
	        $scope.macros = [];
	    };

	    $scope.$on("formSubmitting", function (ev, args) {	
			var syntax = [];
	    	angular.forEach($scope.renderModel, function(value, key){
	    		syntax.push(value.syntax);
	    	});

			$scope.model.value = syntax.join("");
	    });


		function trim(str, chr) {
			var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^'+chr+'+|'+chr+'+$', 'g');
			return str.replace(rgxtrim, '');
		}

});