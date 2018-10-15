//DO NOT DELETE THIS, this is in use... 
angular.module('umbraco')
.controller("Umbraco.PropertyEditors.MacroContainerController",
	
	function($scope, dialogService, entityResource, macroService){

		$scope.renderModel = [];
		$scope.allowOpenButton = true;
		$scope.allowRemoveButton = true;
		$scope.sortableOptions = {};
		
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
					setSortingState($scope.renderModel);
				}
			});
		}

		
		function collectDetails(macro){
			macro.details = "";
			macro.icon = "icon-settings-alt";
			if(macro.macroParamsDictionary){
				angular.forEach((macro.macroParamsDictionary), function(value, key){
					macro.details += key + ": " + value + " ";	
				});	
			}
		}

		function openDialog(index){
			var dialogData = {
			    allowedMacros: $scope.model.config.allowed
			};

			if(index !== null && $scope.renderModel[index]) {
				var macro = $scope.renderModel[index];
				dialogData["macroData"] = macro;
			}

			$scope.macroPickerOverlay = {};
			$scope.macroPickerOverlay.view = "macropicker";
			$scope.macroPickerOverlay.dialogData = dialogData;
			$scope.macroPickerOverlay.show = true;

			$scope.macroPickerOverlay.submit = function(model) {

				var macroObject = macroService.collectValueData(model.selectedMacro, model.macroParams, dialogData.renderingEngine);
				collectDetails(macroObject);

				//update the raw syntax and the list...
				if(index !== null && $scope.renderModel[index]) {
					$scope.renderModel[index] = macroObject;
				} else {
					$scope.renderModel.push(macroObject);
				}

				setSortingState($scope.renderModel);

				$scope.macroPickerOverlay.show = false;
				$scope.macroPickerOverlay = null;
			};

			$scope.macroPickerOverlay.close = function(oldModel) {
				$scope.macroPickerOverlay.show = false;
				$scope.macroPickerOverlay = null;
			};

		}



		$scope.edit =function(index){
				openDialog(index);
		};

		$scope.add = function () {

		    if ($scope.model.config.max && $scope.model.config.max > 0 && $scope.renderModel.length >= $scope.model.config.max) {
                //cannot add more than the max
		        return;
		    }
		    
			openDialog();
		};

		$scope.remove =function(index){
			$scope.renderModel.splice(index, 1);
			setSortingState($scope.renderModel);
		};

	    $scope.clear = function() {
	        $scope.model.value = "";
	        $scope.renderModel = [];
	    };

	    var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {	
			var syntax = [];
	    	angular.forEach($scope.renderModel, function(value, key){
	    		syntax.push(value.syntax);
	    	});

			$scope.model.value = syntax.join("");
	    });

	    //when the scope is destroyed we need to unsubscribe
	    $scope.$on('$destroy', function () {
	        unsubscribe();
	    });


		function trim(str, chr) {
			var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^'+chr+'+|'+chr+'+$', 'g');
			return str.replace(rgxtrim, '');
		}

		function setSortingState(items) {
			// disable sorting if the list only consist of one item
			if(items.length > 1) {
				$scope.sortableOptions.disabled = false;
			} else {
				$scope.sortableOptions.disabled = true;
			}
    	}

});
