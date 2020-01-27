/// <reference path="../../../../../node_modules/monaco-editor/monaco.d.ts" />
(function() {
    'use strict';

    function CodeEditorDirective(assetsService, windowResizeListener, angularHelper) {

        function link(scope, el, attr, ngModel) {

            // Load in Monaco aka VS-Code editor library
            assetsService.load(['lib/monaco-editor/vs/loader.js'], scope).then(function () {
                // OUCH editor.js that this loader polyfills require
                // Is 2MB! - would need to be discussed after POC demo's
                // Adds fair bit of weight
                init();
            });

            function init() {
                require.config({ paths: { 'vs': '/umbraco/lib/monaco-editor/vs' }});
                require(['vs/editor/editor.main'], function() {

                    // An onInit function that could be used to setup & configure custom lang stuff
                    // we pass the top level 'monaco' object/api which allows monaco.languages.json ...
                    if (angular.isFunction(scope.editorOnInit)) {
                        scope.editorOnInit(monaco);
                    }

                    // element in a directive is not raw DOM element but wrapped in jQLite
                    const domEl = el[0];

                    // default options for VSCode editor that makes sense to us
                    // these can be overwritten by setting them on the directive code-editor-config="" atribute
                    const defaults = {
                        lineNumbers: "on",
                        fontSize: 16,
                        showUnused: true,
                        scrollBeyondLastLine: false,
                        minimap: {
                            enabled: false
                        }
                    };

                    let options = scope.codeEditorConfig;

                    // overwrite the defaults if there are any specified
                    angular.extend(defaults, options);

                    //now copy back to the options we will use
                    options = defaults;

                    // const modelUri = monaco.Uri.parse("json://grid/settings.json");
                    // const jsonModel = monaco.editor.createModel("[{}]", "json", modelUri);

                    // monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
                    //     validate: true,
                    //     schemas: [
                    //         {
                    //             uri: "",
                    //             fileMatch: [modelUri.toString()],
                    //             schema: {
                    //                 "definitions": {},
                    //                 "$schema": "",
                    //                 "$id": "http://example.com/root.json",
                    //                 "type": "array",
                    //                 "title": "Umbraco Grid Settings Schema",
                    //                 "default": null,
                    //                 "items": {
                    //                 "$id": "#/items",
                    //                 "type": "object",
                    //                 "title": "Grid Setting",
                    //                 "default": null,
                    //                 "required": [
                    //                     "label",
                    //                     "description",
                    //                     "key",
                    //                     "view"
                    //                 ],
                    //                 "properties": {
                    //                     "label": {
                    //                     "$id": "#/items/properties/label",
                    //                     "type": "string",
                    //                     "title": "The Label Schema",
                    //                     "default": "",
                    //                     "examples": [
                    //                         "Class"
                    //                     ],
                    //                     "pattern": "^(.*)$"
                    //                     },
                    //                     "description": {
                    //                     "$id": "#/items/properties/description",
                    //                     "type": "string",
                    //                     "title": "The Description Schema",
                    //                     "default": "",
                    //                     "examples": [
                    //                         "Set a css class"
                    //                     ],
                    //                     "pattern": "^(.*)$"
                    //                     },
                    //                     "key": {
                    //                     "$id": "#/items/properties/key",
                    //                     "type": "string",
                    //                     "title": "The Key Schema",
                    //                     "default": "",
                    //                     "examples": [
                    //                         "class"
                    //                     ],
                    //                     "pattern": "^(.*)$"
                    //                     },
                    //                     "view": {
                    //                     "$id": "#/items/properties/view",
                    //                     "type": "string",
                    //                     "title": "The View Schema",
                    //                     "default": "",
                    //                     "examples": [
                    //                         "textstring"
                    //                     ],
                    //                     "pattern": "^(.*)$"
                    //                     },
                    //                     "modifier": {
                    //                     "$id": "#/items/properties/modifier",
                    //                     "type": "string",
                    //                     "title": "The Modifier Schema",
                    //                     "default": "",
                    //                     "examples": [
                    //                         "col-sm-{0}"
                    //                     ],
                    //                     "pattern": "^(.*)$"
                    //                     },
                    //                     "applyTo": {
                    //                     "$id": "#/items/properties/applyTo",
                    //                     "type": "string",
                    //                     "title": "The Applyto Schema",
                    //                     "default": "",
                    //                     "examples": [
                    //                         "row|cell"
                    //                     ],
                    //                     "pattern": "^(.*)$"
                    //                     }
                    //                 }
                    //                 }
                    //             }
                    //         }
                    //     ]
                    // });


                    // Init & configure VS Code with options
                    const editor = monaco.editor.create(domEl, options);


                    // Value bind - contents of code editor to set from scope.model
                    // TODO: Value can be set as options property or as a model
                    if(scope.model){
                        //editor.setModel(jsonModel);
                        editor.setValue(scope.model);
                    }

                    // Editor has loaded with contents
                    // An onLoad function that could be used to setup & configure custom lang stuff
                    // we pass the top level 'monaco' object/api which allows monaco.languages.json ...
                    // along with the editor itself we have created
                    if (angular.isFunction(scope.editorOnLoad)) {
                        scope.editorOnLoad(monaco, editor);
                    }

                    // TODO: When options in directive/component change
                    // Update VS Code options (aka WATCH scope.codeEditorConfig)
                    // editor.updateOptions();


                    // When content changes in code editor
                    // Ensure the scope.model that was
                    // passed into directive/component is updated
                    editor.onDidChangeModelContent((e) => {
                        const editorContents = editor.getValue();

                        // TODO: Loads of events firing
                        // Will it hammer perf - should it debounce/delay updating?!
                        angularHelper.safeApply(scope, function () {
                            scope.model = editorContents;
                        });
                    });

                    // Use the windowResizeListener service to listen for window.resize
                    // And ensure we resize/redraw VS Code editor
                    const resizeCallback = function(size) {
                        editor.layout();
                    };
                    windowResizeListener.register(resizeCallback);

                    // When angular removes the DOM element from the page
                    // Lets ensure we tidy up correctly & remove our resize callback
                    // Along with disposing VSCode
                    el.on('$destroy', function() {
                        windowResizeListener.unregister(resizeCallback);
                        editor.dispose();
                    });
                });
            }
        }

        var directive = {
            restrict: 'A',
            scope: {
                "model": "=",
                "codeEditorConfig": "=",
                "editorOnInit": "=",
                "editorOnLoad": "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives')
        .directive('umbCodeEditor', CodeEditorDirective);

})();
