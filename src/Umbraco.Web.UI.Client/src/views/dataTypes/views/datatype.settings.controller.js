/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataType.SettingsController
 * @function
 *
 * @description
 * The controller for the settings view of the datatype editor
 */
 function DataTypeSettingsController($scope, editorService) {

  var vm = this;

  vm.selectedEditorModel = null;
  
  // TODO - get setting value from data type and config
  // 'on': allow the user to change the property editor
  // 'off': don't allow the user to change the property editor
  // 'disabled': don't allow the user to change the property editor, but show a help text where to change the setting
  vm.canChangePropertyEditor = 'on';

  vm.openPropertyEditorPicker = openPropertyEditorPicker;

  function init () {
    vm.selectedEditorModel = $scope.model.content.availableEditors.find(editor => editor.alias === $scope.model.content.selectedEditor);
  }

  function openPropertyEditorPicker () {

    const propertyEditorPicker = {
      view: 'views/common/infiniteeditors/propertyeditorpicker/propertyeditorpicker.html',
      size: 'small',
      propertyEditors: $scope.model.content.availableEditors,
      submit: model => {
        const selected = model.selection[0];
        $scope.model.content.selectedEditor = selected;
        vm.selectedEditorModel = $scope.model.content.availableEditors.find(editor => editor.alias === selected);
        editorService.close();
      },
      close: () => editorService.close()
    };

    editorService.open(propertyEditorPicker);
  }

  init();
}

angular.module("umbraco").controller("Umbraco.Editors.DataType.SettingsController", DataTypeSettingsController);
