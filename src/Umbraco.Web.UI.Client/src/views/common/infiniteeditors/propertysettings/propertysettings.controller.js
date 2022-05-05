/**
 * @ngdoc controller
 * @name Umbraco.Editors.PropertySettingsController
 * @function
 *
 * @description
 * The controller for the content type editor property settings dialog
 */

(function () {
    "use strict";

    function PropertySettingsEditor($scope, contentTypeResource, dataTypeResource, dataTypeHelper, formHelper, localizationService, userService, editorService, propertyTypeResource) {

        var vm = this;

        const dataTypesCanBeChangedConfig = window.Umbraco.Sys.ServerVariables.umbracoSettings.dataTypesCanBeChanged;
        
        vm.allowChangeDataType = false;
        vm.changeDataTypeHelpTextIsVisible = false;
        vm.propertyTypeHasValues = false;

        vm.showValidationPattern = false;
        vm.focusOnPatternField = false;
        vm.focusOnMandatoryField = false;
        vm.selectedValidationType = null;
        vm.validationTypes = [];
        vm.labels = {};

        vm.changeValidationType = changeValidationType;
        vm.changeValidationPattern = changeValidationPattern;
        vm.openDataTypePicker = openDataTypePicker;
        vm.openDataTypeSettings = openDataTypeSettings;
        vm.submitOnEnter = submitOnEnter;
        vm.submit = submit;
        vm.close = close;

        vm.toggleAllowCultureVariants = toggleAllowCultureVariants;
        vm.toggleAllowSegmentVariants = toggleAllowSegmentVariants;
        vm.toggleValidation = toggleValidation;
        vm.toggleShowOnMemberProfile = toggleShowOnMemberProfile;
        vm.toggleMemberCanEdit = toggleMemberCanEdit;
        vm.toggleIsSensitiveData = toggleIsSensitiveData;
        vm.toggleLabelOnTop = toggleLabelOnTop;

        function onInit() {

            userService.getCurrentUser().then(function(user) {
                vm.showSensitiveData = user.userGroups.indexOf("sensitiveData") != -1;
            });

            const propertyAlias = $scope.model.property.alias;

            if (propertyAlias && (dataTypesCanBeChangedConfig === "False" || dataTypesCanBeChangedConfig === "FalseWithHelpText")) {
                propertyTypeResource.hasValues(propertyAlias)
                .then(data => {
                    vm.propertyTypeHasValues = data.hasValues;
                    vm.allowChangeDataType = !vm.propertyTypeHasValues;
                    vm.changeDataTypeHelpTextIsVisible = !vm.allowChangeDataType && dataTypesCanBeChangedConfig === "FalseWithHelpText";
                });
            } else {
                vm.allowChangeDataType = true;
            }

            //make the default the same as the content type
            if (!$scope.model.property.dataTypeId) {
                $scope.model.property.allowCultureVariant = $scope.model.contentTypeAllowCultureVariant;
            }

            loadValidationTypes();

        }

        function loadValidationTypes() {

            var labels = [
                "validation_validateAsEmail",
                "validation_validateAsNumber",
                "validation_validateAsUrl",
                "validation_enterCustomValidation",
                "validation_fieldIsMandatory",
                "contentTypeEditor_displaySettingsLabelOnTop"
            ];

            localizationService.localizeMany(labels)
                .then(function(data){

                    vm.labels.validateAsEmail = data[0];
                    vm.labels.validateAsNumber = data[1];
                    vm.labels.validateAsUrl = data[2];
                    vm.labels.customValidation = data[3];
                    vm.labels.fieldIsMandatory = data[4];
                    vm.labels.displaySettingsLabelOnTop = data[5];

                    vm.validationTypes = [
                        {
                            "name": vm.labels.validateAsEmail,
                            "key": "email",
                            "pattern": "[a-zA-Z0-9_\.\+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-\.]+",
                            "enableEditing": true
                        },
                        {
                            "name": vm.labels.validateAsNumber,
                            "key": "number",
                            "pattern": "^[0-9]*$",
                            "enableEditing": true
                        },
                        {
                            "name": vm.labels.validateAsUrl,
                            "key": "url",
                            "pattern": "https?\:\/\/[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,}",
                            "enableEditing": true
                        },
                        {
                            "name": vm.labels.customValidation,
                            "key": "custom",
                            "pattern": "",
                            "enableEditing": true
                        }
                    ];

                    matchValidationType();

                });

        }

        function changeValidationPattern() {
            matchValidationType();
        }

        function openDataTypePicker(property) {

            vm.focusOnMandatoryField = false;

            var dataTypePicker = {
                property: $scope.model.property,
                contentTypeName: $scope.model.contentTypeName,
                view: "views/common/infiniteeditors/datatypepicker/datatypepicker.html",
                size: "medium",
                submit: function(model) {

                    $scope.model.updateSameDataTypes = model.updateSameDataTypes;

                    vm.focusOnMandatoryField = true;

                    // update property
                    property.config = model.property.config;
                    property.editor = model.property.editor;
                    property.view = model.property.view;
                    property.dataTypeId = model.property.dataTypeId;
                    property.dataTypeIcon = model.property.dataTypeIcon;
                    property.dataTypeName = model.property.dataTypeName;

                    editorService.close();
                },
                close: function(model) {
                    editorService.close();
                }
            };

            editorService.open(dataTypePicker);

        }

        function openDataTypeSettings(property) {

            vm.focusOnMandatoryField = false;

            var dataTypeSettings = {
                view: "views/common/infiniteeditors/datatypesettings/datatypesettings.html",
                id: property.dataTypeId,
                submit: function(model) {
                    contentTypeResource.getPropertyTypeScaffold(model.dataType.id).then(function (propertyType) {
                        // update editor
                        property.config = propertyType.config;
                        property.editor = propertyType.editor;
                        property.view = propertyType.view;
                        property.dataTypeId = model.dataType.id;
                        property.dataTypeIcon = model.dataType.icon;
                        property.dataTypeName = model.dataType.name;

                        // set flag to update same data types
                        $scope.model.updateSameDataTypes = true;

                        vm.focusOnMandatoryField = true;

                        editorService.close();
                    });
                },
                close: function() {
                    editorService.close();
                }
            };

            editorService.open(dataTypeSettings);

        }

        function submitOnEnter(event) {
            if(event && event.keyCode === 13) {
                submit();
            }
        }

        function submit() {
            if($scope.model.submit) {
                if (formHelper.submitForm({scope: $scope})) {
                    $scope.model.submit($scope.model);
                }
            }
        }

        function close() {
            if($scope.model.close) {
                $scope.model.close();
            }
        }

        function matchValidationType() {

            if ($scope.model.property.validation.pattern !== null && $scope.model.property.validation.pattern !== "" && $scope.model.property.validation.pattern !== undefined) {

                var match = false;

                // find and show if a match from the list has been chosen
                vm.validationTypes.forEach(function (validationType, index) {
                    if ($scope.model.property.validation.pattern === validationType.pattern) {
                        vm.selectedValidationType = vm.validationTypes[index];
                        vm.showValidationPattern = true;
                        match = true;
                    }
                });

                // if there is no match - choose the custom validation option.
                if (!match) {
                    vm.validationTypes.forEach(function (validationType) {
                        if (validationType.key === "custom") {
                            vm.selectedValidationType = validationType;
                            vm.showValidationPattern = true;
                        }
                    });
                }
            }

        }

        function changeValidationType(selectedValidationType) {

            if (selectedValidationType) {
                $scope.model.property.validation.pattern = selectedValidationType.pattern;
                vm.showValidationPattern = true;

                // set focus on textarea
                if (selectedValidationType.key === "custom") {
                    vm.focusOnPatternField = true;
                }

            } else {
                $scope.model.property.validation.pattern = "";
                vm.showValidationPattern = false;
            }

        }

        function toggleValue(settingValue) {
            return !settingValue;
        }

        function toggleAllowCultureVariants() {
            $scope.model.property.allowCultureVariant = toggleValue($scope.model.property.allowCultureVariant);
        }

        function toggleAllowSegmentVariants() {
            $scope.model.property.allowSegmentVariant = toggleValue($scope.model.property.allowSegmentVariant);
        }

        function toggleValidation() {
            $scope.model.property.validation.mandatory = toggleValue($scope.model.property.validation.mandatory);
        }

        function toggleShowOnMemberProfile() {
            $scope.model.property.showOnMemberProfile = toggleValue($scope.model.property.showOnMemberProfile);
        }

        function toggleMemberCanEdit() {
            $scope.model.property.memberCanEdit = toggleValue($scope.model.property.memberCanEdit);
        }

        function toggleIsSensitiveData() {
            $scope.model.property.isSensitiveData = toggleValue($scope.model.property.isSensitiveData);
        }
        function toggleLabelOnTop() {
            $scope.model.property.labelOnTop = toggleValue($scope.model.property.labelOnTop);
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.PropertySettingsController", PropertySettingsEditor);

})();
