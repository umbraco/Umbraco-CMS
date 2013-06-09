//Sample file to simplify setting up a property editor
//without any serverside processing

//defines that the below JS code needs angular to be loaded
//this is strictly a require.js convention
//we are working in making that simpler (and been advised to ditch requirejs)

define(['app','angular'], function (app, angular) {

	//Next we get the umbraco module, so we can register a controller on it
	angular.module('umbraco')
		.controller("Sample.PropertyEditorController", function($scope, $log, dialog){
			
			//we could then output the scope data to the log:
			$log.log($scope.model);

			//or we could set the model value
			$scope.model.value = "Test data";

			//we could have a function:
			//which appends something to the model data
			$scope.doStuff = function(input){
				$scope.model.value = input + " Appended value";
			};

			//we could use one of the internal services to wire up a dialog
			$scope.openDialog = function(){
				var d = dialog.mediaPicker({scope: $scope, callback: myCallBack});		
			};

			function myCallBack(data){
				$.each(data.selection, function(i, img){
					$log.log(img);
					$scope.model.value += img.url;	
				});
			}
		});

	return angular;
});