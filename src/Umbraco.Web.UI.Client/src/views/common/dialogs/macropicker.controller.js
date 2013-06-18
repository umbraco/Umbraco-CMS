//used for the macro picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.MacroPickerController", function ($scope, macroFactory, umbPropEditorHelper) {
	$scope.macros = macroFactory.all(true);
	$scope.dialogMode = "list";

	$scope.configureMacro = function(macro){
		$scope.dialogMode = "configure";
		$scope.dialogData.macro = macroFactory.getMacro(macro.alias);
	    //set the correct view for each item
		for (var i = 0; i < dialogData.macro.properties.length; i++) {
		    dialogData.macro.properties[i].editorView = umbPropEditorHelper.getViewPath(dialogData.macro.properties[i].view);
		}
	};
});