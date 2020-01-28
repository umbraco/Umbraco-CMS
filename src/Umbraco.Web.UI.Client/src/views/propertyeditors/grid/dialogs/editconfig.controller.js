/// <reference path="../../../../../node_modules/monaco-editor/monaco.d.ts" />
function EditConfigController($scope) {

    var vm = this;

    vm.submit = submit;
    vm.close = close;

    vm.codeEditorOptions = {
        language: "json",
        formatOnType:true
    }

    let jsonModel;

    vm.onInit = function(monaco) {
        const modelUri = monaco.Uri.parse("json://grid/settings.json");

        // TODO: If we set the value of editor & not model attribute on div
        // How will I keep the code editor in sync with $scope.model.config !?
        // OR put differently - how will I save the value when clicking Save/Done button

        // Answer: Get the VALUE from VSCode editor when saving as opposed to updating model from VSCode own events

        // angular.toJson (this removes stuff like $$hashKey for us)
        // as the code contents needs to be a string & not the raw JSON object
        jsonModel = monaco.editor.createModel(angular.toJson($scope.model.config), "json", modelUri);

        // TODO: Improve the JSON schema from the online generated tool
        monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
            validate: true,
            schemas: [{
                uri: "",
                fileMatch: [modelUri.toString()],
                schema: {
                    "definitions": {},
                    "$schema": "",
                    "$id": "http://example.com/root.json",
                    "type": "array",
                    "title": "Umbraco Grid Settings Schema WARREN",
                    "default": null,
                    "items": {
                        "$id": "#/items",
                        "type": "object",
                        "title": "Grid Setting",
                        "default": null,
                        "required": [
                            "label",
                            "description",
                            "key",
                            "view"
                        ],
                        "properties": {
                            "label": {
                                "$id": "#/items/properties/label",
                                "type": "string",
                                "title": "The Label Schema",
                                "default": "",
                                "examples": [
                                    "Class"
                                ],
                                "pattern": "^(.*)$"
                            },
                            "description": {
                                "$id": "#/items/properties/description",
                                "type": "string",
                                "title": "The Description Schema",
                                "default": "",
                                "examples": [
                                    "Set a css class"
                                ],
                                "pattern": "^(.*)$"
                            },
                            "key": {
                                "$id": "#/items/properties/key",
                                "type": "string",
                                "title": "The Key Schema",
                                "default": "",
                                "examples": [
                                    "class"
                                ],
                                "pattern": "^(.*)$"
                            },
                            "view": {
                                "$id": "#/items/properties/view",
                                "type": "string",
                                "title": "The View Schema",
                                "default": "",
                                "examples": [
                                    "textstring"
                                ],
                                "pattern": "^(.*)$"
                            },
                            "modifier": {
                                "$id": "#/items/properties/modifier",
                                "type": "string",
                                "title": "The Modifier Schema",
                                "default": "",
                                "examples": [
                                    "col-sm-{0}"
                                ],
                                "pattern": "^(.*)$"
                            },
                            "applyTo": {
                                "$id": "#/items/properties/applyTo",
                                "type": "string",
                                "title": "The Applyto Schema",
                                "default": "",
                                "examples": [
                                    "row|cell"
                                ],
                                "pattern": "^(.*)$"
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

        // TODO: YUK see if we can remove timeout
        // As waiting for the model/content to be set is weird
        // Must be some nice native event?!
        // ALSO ITS SUPER JARRING WITH FLASH OF CHANGE
        setTimeout(function() {
            editor.getAction('editor.action.formatDocument').run();
        }, 100);
    }

    function submit() {
        if ($scope.model && $scope.model.submit) {

            // Lets check & verify its valid JSON
            const jsonCode = jsonModel.getValue();

            if(isValidJson(jsonCode) === false) {
                // TODO: Show some UI to wanr user
                alert('INVALID TRY AGAIN!');
            } else {
                // We need to dispose the model - as if we re-open
                // VSCode errors with 'ModelService: Cannot add model because it already exists!'
                jsonModel.dispose();

                // We manually assign ithe code editor contents when saving & closing
                // As this implementation for this JSON editor uses
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
