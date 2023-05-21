/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataTypeConfigurationPickerController
 * @function
 *
 * @description
 * The controller for the content type editor data type configuration picker dialog
 */

(function() {
    "use strict";

    function DataTypeConfigurationPicker($scope, $filter, dataTypeResource, dataTypeHelper, contentTypeResource, localizationService, editorService) {
        
        var vm = this;
        
        vm.configs = [];
        
        vm.loading = true;
        
        vm.newDataType = newDataType;
        vm.pickDataType = pickDataType;
        vm.close = close;

        function activate() {
            setTitle();
            load();
        }

        function setTitle() {
            if (!$scope.model.title) {
                localizationService.localize("defaultdialogs_selectEditorConfiguration")
                    .then(function(data){
                        $scope.model.title = data;
                    });
            }
        }

        function load() {

            dataTypeResource.getGroupedDataTypes().then(function(configs) {
                
                var filteredConfigs = [];
                
                Object.values(configs).forEach(configGroup => {
                    for(var i = 0; i < configGroup.length; i++) {
                        if (configGroup[i].alias === $scope.model.editor.alias) {
                            filteredConfigs.push(configGroup[i]);
                        }
                    }
                });
            
                vm.configs = filteredConfigs;
                vm.loading = false;
            });

        }
        
        function newDataType() {

            var dataTypeSettings = {
                propertyEditor: $scope.model.editor,
                property: $scope.model.property,
                contentTypeName: $scope.model.contentTypeName,
                create: true,
                view: "views/common/infiniteeditors/datatypesettings/datatypesettings.html",
                submit: function(model) {
                    contentTypeResource.getPropertyTypeScaffold(model.dataType.id).then(function(propertyType) {
                        $scope.model.submit(model.dataType, propertyType, true);
                        editorService.close();
                    });
                },
                close: function() {
                    editorService.close();
                }
            };

            editorService.open(dataTypeSettings);

        }

        function pickDataType(selectedConfig) {

            selectedConfig.loading = true;
            dataTypeResource.getById(selectedConfig.id).then(function(dataType) {
                contentTypeResource.getPropertyTypeScaffold(dataType.id).then(function(propertyType) {
                    selectedConfig.loading = false;
                    $scope.model.submit(dataType, propertyType, false);
                });
            });
        }
        
        function close() {
            if($scope.model.close) {
                $scope.model.close();
            }
        }

        activate();

    }

    angular.module("umbraco").controller("Umbraco.Editors.DataTypeConfigurationPickerController", DataTypeConfigurationPicker);

})();
