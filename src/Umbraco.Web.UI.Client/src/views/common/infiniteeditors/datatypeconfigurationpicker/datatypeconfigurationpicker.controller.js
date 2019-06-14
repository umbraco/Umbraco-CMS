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
        
        vm.newConfig = newConfig;
        vm.pickConfig = pickConfig;
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
                
                console.log("configs:");
                console.log(configs);
                
                
                console.log("$scope.model")
                console.log($scope.model)
                
                var filteredConfigs = [];
                
                _.each(configs, function(configGroup) {
                        for(var i = 0; i<configGroup.length; i++) {
                            if (configGroup[i].alias === $scope.model.dataType.alias) {
                                filteredConfigs.push(configGroup[i]);
                            }
                        }
                    }
                );
                
                vm.configs = filteredConfigs;
                vm.loading = false;
            });

        }
        
        function newConfig(propertyEditor) {

            var dataTypeSettings = {
                propertyEditor: propertyEditor,
                property: $scope.model.property,
                contentTypeName: $scope.model.contentTypeName,
                create: true,
                view: "views/common/infiniteeditors/datatypesettings/datatypesettings.html",
                submit: function(model) {
                    contentTypeResource.getPropertyTypeScaffold(model.dataType.id).then(function(propertyType) {
                        submit(model.dataType, propertyType, true);
                        editorService.close();
                    });
                },
                close: function() {
                    editorService.close();
                }
            };

            editorService.open(dataTypeSettings);

        }

        function pickConfig(selectedConfig) {
            selectedConfig.loading = true;
            console.log("pickConfig:", selectedConfig)
            dataTypeResource.getById(selectedConfig.id).then(function(dataType) {
                contentTypeResource.getPropertyTypeScaffold(dataType.id).then(function(propertyType) {
                    selectedConfig.loading = false;
                    submit(dataType, propertyType, false);
                });
            });
        }
        
        function submit(dataType, propertyType, isNew) {
            // update property
            $scope.model.property.config = propertyType.config;
            $scope.model.property.editor = propertyType.editor;
            $scope.model.property.view = propertyType.view;
            $scope.model.property.dataTypeId = dataType.id;
            $scope.model.property.dataTypeIcon = dataType.icon;
            $scope.model.property.dataTypeName = dataType.name;

            $scope.model.updateSameDataTypes = isNew;
            
            console.log("model submit");
            
            $scope.model.submit($scope.model);
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
