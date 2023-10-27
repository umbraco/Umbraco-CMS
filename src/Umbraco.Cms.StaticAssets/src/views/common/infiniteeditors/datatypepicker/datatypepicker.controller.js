/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataTypePickerController
 * @function
 *
 * @description
 * The controller for the content type editor data type picker dialog
 */

(function() {
    "use strict";

    function DataTypePicker($scope, $filter, dataTypeResource, contentTypeResource, localizationService, editorService) {

        var vm = this;
        
        vm.showDataTypes = true;
        vm.dataTypes = [];
        vm.loading = true;
        vm.loadingConfigs = false;
        vm.searchTerm = "";
        vm.searchResult = null;
        
        vm.viewOptionsForEditor = viewOptionsForEditor;
        vm.pickDataType = pickDataType;
        vm.pickEditor = pickEditor;
        vm.close = close;
        vm.searchTermChanged = searchTermChanged;

        function activate() {
            setTitle();
            loadTypes();
        }

        function setTitle() {
            if (!$scope.model.title) {
                localizationService.localize("defaultdialogs_selectEditor")
                    .then(function(data){
                        $scope.model.title = data;
                    }
                );
            }
        }
        
        function loadTypes() {
            
            dataTypeResource.getGroupedPropertyEditors().then(function(dataTypes) {
                vm.dataTypes = dataTypes;
                vm.loading = false;
            });

        }
        
        function loadConfigurations() {
            
            vm.loading = true;
            vm.loadingConfigs = true;
            
            dataTypeResource.getGroupedDataTypes().then(function(configs) {
                vm.configs = configs;
                vm.loading = false;
                performeSearch();
            });

        }

        
        function searchTermChanged() {
            
            vm.showDataTypes = (vm.searchTerm === "");
            
            if(vm.loadingConfigs !== true) {
                loadConfigurations()
            } else {
                performeSearch();
            }
            
        }
        
        function performeSearch() {
            
            if (vm.searchTerm) {
                if (vm.configs) {
                    
                    var regex = new RegExp(vm.searchTerm, "i");
                    vm.searchResult = {
                        configs: filterCollection(vm.configs, regex),
                        dataTypes: filterCollection(vm.dataTypes, regex)
                    };
                }
            } else {
                vm.searchResult = null;
            }
            
        }
        
        function filterCollection(collection, regex) {
            return _.map(_.keys(collection), function (key) {
                return {
                    group: key,
                    entries: $filter('filter')(collection[key], function (dataType) {
                        return regex.test(dataType.name) || regex.test(dataType.alias);
                    })
                }
            });
        }

        
        function viewOptionsForEditor(editor) {
            
            var dataTypeConfigurationPicker = {
                editor: editor,
                property: $scope.model.property,
                contentTypeName: $scope.model.contentTypeName,
                view: "views/common/infiniteeditors/datatypeconfigurationpicker/datatypeconfigurationpicker.html",
                size: "small",
                submit: function(dataType, propertyType, isNew) {
                    submit(dataType, propertyType, isNew);
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };

            editorService.open(dataTypeConfigurationPicker);

        }

        function pickDataType(selectedDataType) {
            selectedDataType.loading = true;
            dataTypeResource.getById(selectedDataType.id).then(function(dataType) {
                contentTypeResource.getPropertyTypeScaffold(dataType.id).then(function(propertyType) {
                    selectedDataType.loading = false;
                    submit(dataType, propertyType, false);
                });
            });
        }

        function pickEditor(propertyEditor) {

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

        function submit(dataType, propertyType, isNew) {
            // update property
            $scope.model.property.config = propertyType.config;
            $scope.model.property.editor = propertyType.editor;
            $scope.model.property.view = propertyType.view;
            $scope.model.property.dataTypeId = dataType.id;
            $scope.model.property.dataTypeIcon = dataType.icon;
            $scope.model.property.dataTypeName = dataType.name;

            $scope.model.updateSameDataTypes = isNew;

            $scope.model.submit($scope.model);
        }
        
        function close() {
            if($scope.model.close) {
                $scope.model.close();
            }
        }

        activate();

    }

    angular.module("umbraco").controller("Umbraco.Editors.DataTypePickerController", DataTypePicker);

})();
