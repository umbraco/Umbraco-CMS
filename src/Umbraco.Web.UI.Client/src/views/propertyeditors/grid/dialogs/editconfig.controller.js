/// <reference path="../../../../../node_modules/monaco-editor/monaco.d.ts" />
function EditConfigController($scope) {

    var vm = this;

    vm.submit = submit;
    vm.close = close;

    vm.invalidJson = false;

    vm.codeEditorOptions = {
        language: "json",
        formatOnType:true
    }

    let jsonModel;

    vm.onInit = function(monaco) {

        // Need to make a fake URI to associate to our model so we get JSON Schema for this URI
        const modelUri = monaco.Uri.parse("json://grid/settings.json");

        // angular.toJson (this removes stuff like $$hashKey for us)
        // as the code contents needs to be a string & not the raw JSON object
        jsonModel = monaco.editor.createModel(angular.toJson($scope.model.config, true), "json", modelUri);

        // TODO: Improve the JSON schema from the online generated tool
        monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
            validate: true,
            schemas: [{
                uri: "",
                fileMatch: [modelUri.toString()],
                schema: {
                    "$schema": "http://json-schema.org/draft-07/schema#",
                    "title": "Umbraco Grid Settings Schema",
                    "description": "A JSON array/collection of settings and style configurations for the grid editor",
                    "type": "array",
                    "items": {
                        "type": "object",
                        "title": "Editable grid setting",
                        "required": [
                            "label",
                            "description",
                            "key",
                            "view"
                        ],
                        "properties": {
                            "label": {
                                "type": "string",
                                "title": "Field name displayed in the content editor UI",
                                "examples": [
                                    "Background image",
                                    "Title",
                                    "Custom data"
                                ]
                            },
                            "description": {
                                "type": "string",
                                "title": "Descriptive text displayed in the content editor UI to guide the user",
                                "examples": [
                                    "Choose an image",
                                    "Set a title on this element",
                                    "Set the custom data on this element"
                                ]
                            },
                            "key": {
                                "type": "string",
                                "title": "The key the entered setting value will be stored under",
                                "examples": [
                                    "background-image",
                                    "title",
                                    "data-custom"
                                ]
                            },
                            "view": {
                                "type": "string",
                                "title": "The prevalue editor used to enter a setting value with",
                                "description": "This can be a value such as 'mediapicker', 'textstring' or '/app_plugins/grid/editors/view.html'",
                                "examples": [
                                    "imagepicker",
                                    "textstring",
                                    "radiobuttonlist"
                                ]
                            },
                            "prevalues": {
                                "type": "array",
                                "title": "An array of prevalues to use with the prevalue editor 'value' property",
                                "minItems": 1,
                                "uniqueItems": true,
                                "items": {
                                    "type": "object",
                                    "properties": {
                                        "label": {
                                            "type": "string",
                                            "title": "The label shown for the prevalue"
                                        },
                                        "value": {
                                            "type": "string",
                                            "title": "The value used for the prevalue"
                                        }
                                    },
                                    "required": [ "label", "value" ]
                                }
                            },
                            "modifier": {
                                "type": "string",
                                "title": "A format to prepend, append or wrap the value from the editor in a string",
                                "description": "This is optional and gives you more control of the output",
                                "examples": ["url('{0}')"]
                            },
                            "applyTo": {
                                "title": "Defines what this setting can be applied to, be it a row or cell or specific configurations.",
                                "description": "States whether the setting can be used on a cell or a row. If either not present or empty, the setting will be shown both on Rows and Cells.",
                                "anyOf": [
                                    {
                                        "type": "object",
                                        "properties": {
                                            "row": {
                                                "type": "string",
                                                "title": "Specify Row names to restrict this to",
                                                "examples": [
                                                    "Headline",
                                                    "Headline,Article"
                                                ]
                                            },
                                            "cell": {
                                                "type": "string",
                                                "title": "Specify Cell numbers to restrict this to",
                                                "examples": [
                                                    "4",
                                                    "4,8,6"
                                                ]
                                            }
                                        },
                                        "minProperties": 1,
                                        "maxProperties": 2
                                    },
                                    {
                                        "type": "string",
                                        "enum": [ "row", "cell" ],
                                        "examples": [
                                            "row",
                                            "cell"
                                        ]
                                    }
                                ]
                            }
                        }
                    }
                }

            }]
        });
    }


    vm.onLoad = function(monaco, editor){
        // We use a model that contains the language & unique key
        // For the JSON schema to apply
        editor.setModel(jsonModel);

        // Will give us a list of errors & warnings
        // We can prevent saving if we have one or more errors
        editor.onDidChangeModelDecorations(() => {

            const modelOwner = jsonModel.getModeId();
            const markers = monaco.editor.getModelMarkers({owner: modelOwner});

            if(markers.length > 0){
                // Set form to invalid - will make the message bar SHOW
                vm.gridConfigEditor.$setValidity('json', false);

                // Set button to disabled
                vm.invalidJson = true;
            } else {
                // Set form to valid - will make the message bar HIDE
                vm.gridConfigEditor.$setValidity('json', true);

                // Set button to active
                vm.invalidJson = false;
            }
        });

    }

    function submit() {
        if ($scope.model && $scope.model.submit) {

            // Lets check & verify its valid JSON
            const jsonCode = jsonModel.getValue();

            // Still need this as the events for onDidChangeModelDecorations can be slow at times
            if(isValidJson(jsonCode) === false) {

                // Set form to invalid - will make the message bar SHOW
                vm.gridConfigEditor.$setValidity('json', false);

                // Let's not close & submit
                return;
            } else {
                // We need to dispose the model - as if we re-open
                // VSCode errors with 'ModelService: Cannot add model because it already exists!'
                jsonModel.dispose();

                // We manually assign ithe code editor contents when saving & closing
                // As opposed to using the attribute editor-contents to be an auto-updated Angular property
                $scope.model.config = angular.fromJson(jsonCode);
                $scope.model.submit($scope.model);

            }
        }
    }

    function close() {
        if ($scope.model.close) {
            // We need to dispose the model - as if we re-open
            // VSCode errors with 'ModelService: Cannot add model because it already exists!'
            jsonModel.dispose();

            $scope.model.close();
        }
    }

    function isValidJson(jsonString) {
        let isValid = true;
        try {
            angular.fromJson(jsonString);
        } catch (err) {
            isValid = false;
        }
        return isValid;
    }
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.GridPrevalueEditor.EditConfigController", EditConfigController);
