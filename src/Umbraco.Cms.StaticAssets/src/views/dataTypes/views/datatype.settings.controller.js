/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataType.SettingsController
 * @function
 *
 * @description
 * The controller for the settings view of the datatype editor
 */
 function DataTypeSettingsController($scope, $routeParams, dataTypeResource, editorService) {

  var vm = this;

  vm.selectedEditorModel = null;

  const dataTypeId = $routeParams.id;
  const dataTypesCanBeChangedConfig = window.Umbraco.Sys.ServerVariables.umbracoSettings.dataTypesCanBeChanged;

  vm.allowChangePropertyEditor = false;
  vm.changePropertyEditorHelpTextIsVisible = false;
  vm.dataTypeHasValues = false;

  vm.openPropertyEditorPicker = openPropertyEditorPicker;

  function init () {
    vm.selectedEditorModel = $scope.model.content.availableEditors.find(editor => editor.alias === $scope.model.content.selectedEditor);

    if (dataTypeId !== "-1" && (dataTypesCanBeChangedConfig === "False" || dataTypesCanBeChangedConfig === "FalseWithHelpText")) {

      // We always allow changing the property editor if the data type is not in use.
      dataTypeResource.hasValues($routeParams.id)
        .then(data => {
          vm.dataTypeHasValues = data.hasValues;
          vm.allowChangePropertyEditor = !vm.dataTypeHasValues;
          vm.changePropertyEditorHelpTextIsVisible = !vm.allowChangePropertyEditor && dataTypesCanBeChangedConfig === "FalseWithHelpText";
        });
    } else {
      vm.allowChangePropertyEditor = true;
    }
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
