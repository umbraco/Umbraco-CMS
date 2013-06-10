//used for the macro picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.MacroPickerController", function ($scope, macroFactory) {	
	$scope.macros = macroFactory.all(true);
	$scope.dialogMode = "list";

	$scope.configureMacro = function(macro){
		$scope.dialogMode = "configure";
		$scope.dialogData.macro = macroFactory.getMacro(macro.alias);
	};
});