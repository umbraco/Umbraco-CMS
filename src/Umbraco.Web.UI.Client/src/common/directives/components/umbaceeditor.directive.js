// TODO: Make sure this does not break anything.
var require = { paths: { 'vs': 'lib/monaco-editor' } };
//var require = { paths: { 'vs': '../node_modules/monaco-editor/min/vs' } };

(function ()
{
    'use strict';

    function MonacoEditorDirective(umbMonacoEditorConfig, assetsService, angularHelper)
    {
        console.log('love');
        /**
         * Sets editor options such as the wrapping mode or the syntax checker.
         *
         * The supported options are:
         *
         *   <ul>
         *     <li>showGutter</li>
         *     <li>useWrapMode</li>
         *     <li>onLoad</li>
         *     <li>theme</li>
         *     <li>mode</li>
         *   </ul>
         *
         * @param acee
         * @param session ACE editor session
         * @param {object} opts Options to be set
         */
        var setOptions = function (monacoEditor, opts)
        {

            // sets the ace worker path, if running from concatenated
            // or minified source
            // if (angular.isDefined(opts.workerPath)) {
            //     // var config = window.ace.require('ace/config');
            //     // config.set('workerPath', opts.workerPath);
            // }


            // ace requires loading
            // if (angular.isDefined(opts.require)) {
            //     opts.require.forEach(function(n) {
            //         window.ace.require(n);
            //     });
            // }

            monacoEditor.updateOptions({
                lineNumbers: "on",
                language: opts.mode ? opts.mode : null,
                wordWrap: opts.useWrapMode ? "on" : "off" // "off" | "on" | "wordWrapColumn" | "bounded"
            });

            // // Boolean options
            // if (angular.isDefined(opts.showGutter)) {
            //     acee.renderer.setShowGutter(opts.showGutter);
            // }
            // if (angular.isDefined(opts.useWrapMode)) {
            //     session.setUseWrapMode(opts.useWrapMode);
            // }
            // if (angular.isDefined(opts.showInvisibles)) {
            //     acee.renderer.setShowInvisibles(opts.showInvisibles);
            // }
            // if (angular.isDefined(opts.showIndentGuides)) {
            //     acee.renderer.setDisplayIndentGuides(opts.showIndentGuides);
            // }
            // if (angular.isDefined(opts.useSoftTabs)) {
            //     session.setUseSoftTabs(opts.useSoftTabs);
            // }
            // if (angular.isDefined(opts.showPrintMargin)) {
            //     acee.setShowPrintMargin(opts.showPrintMargin);
            // }

            // commands
            if (angular.isDefined(opts.disableSearch) && opts.disableSearch)
            {
                // monacoEditor.commands.addCommands([{
                //     name: 'unfind',
                //     bindKey: {
                //         win: 'Ctrl-F',
                //         mac: 'Command-F'
                //     },
                //     exec: function() {
                //         return false;
                //     },
                //     readOnly: true
                // }]);
            }

            // Basic options
            if (angular.isString(opts.theme))
            {
                monacoEditor.updateOptions({ theme: opts.theme }); // vs | vs-dark
            }
            if (angular.isString(opts.mode))
            {
                monacoEditor.updateOptions({ language: opts.mode })
            }
            // Advanced options
            // if (angular.isDefined(opts.firstLineNumber))
            // {
            //     if (angular.isNumber(opts.firstLineNumber))
            //     {
            //         session.setOption('firstLineNumber', opts.firstLineNumber);
            //     } else if (angular.isFunction(opts.firstLineNumber))
            //     {
            //         session.setOption('firstLineNumber', opts.firstLineNumber());
            //     }
            // }

            // advanced options
            var key, obj;
            if (angular.isDefined(opts.advanced))
            {
                for (key in opts.advanced)
                {
                    // create a javascript object with the key and value
                    obj = {
                        name: key,
                        value: opts.advanced[key]
                    };
                    // try to assign the option to the ace editor
                    monacoEditor.updateOptions({ [obj.name]: obj.value });
                }
            }

            // advanced options for the renderer
            // if (angular.isDefined(opts.rendererOptions))
            // {
            //     for (key in opts.rendererOptions)
            //     {
            //         // create a javascript object with the key and value
            //         obj = {
            //             name: key,
            //             value: opts.rendererOptions[key]
            //         };
            //         // try to assign the option to the ace editor
            //         monacoEditor.updateOptions({ [obj.name]: obj.value });
            //     }
            // }

            // onLoad callbacks
            angular.forEach(opts.callbacks, function (cb)
            {
                if (angular.isFunction(cb))
                {
                    cb(monacoEditor);
                }
            });
        };

        function link(scope, el, attr, ngModel)
        {
            assetsService.load([
                "lib/monaco-editor/loader.js"], scope)
                .then(function ()
                {
                    window.require.config({ paths: { 'vs': 'lib/monaco-editor' } });
                    window.require(['vs/editor/editor.main'], function ()
                    {
                        setTimeout(() =>
                        {
                            if (angular.isUndefined(window.monaco))
                            {
                                throw new Error('Monaco editor is not defined.');
                            }
                            else
                            {
                                // init editor
                                init();
                            }
                        }, 200);
                    });
                });

            function init()
            {

                /**
                 * Corresponds the umbMonacoEditorConfig ACE configuration.
                 * @type object
                 */
                const options = umbMonacoEditorConfig || {};

                /**
                 * umbMonacoEditorConfig merged with user options via json in attribute or data binding
                 * @type object
                 */
                let opts = angular.extend({}, options, scope.umbMonacoEditor);


                /**
                 * Monaco editor
                 * @type object https://microsoft.github.io/monaco-editor/api/interfaces/monaco.editor.istandalonecodeeditor.html
                 */
                let editor = monaco.editor.create(el[0], opts);

                /**
                 * Reference to a change listener created by the listener factory.
                 * @function
                 * @see listenerFactory.onChange
                 */
                let onChangeListener;

                /**
                 * Reference to a blur listener created by the listener factory.
                 * @function
                 * @see listenerFactory.onBlur
                 */
                let onBlurListener;

                /**
                 * Calls a callback by checking its existing. The argument list
                 * is variable and thus this function is relying on the arguments
                 * object.
                 * @throws {Error} If the callback isn't a function
                 */
                var executeUserCallback = function ()
                {

                    /**
                     * The callback function grabbed from the array-like arguments
                     * object. The first argument should always be the callback.
                     *
                     * @see [arguments]{@link https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Functions_and_function_scope/arguments}
                     * @type {*}
                     */
                    var callback = arguments[0];

                    /**
                     * Arguments to be passed to the callback. These are taken
                     * from the array-like arguments object. The first argument
                     * is stripped because that should be the callback function.
                     *
                     * @see [arguments]{@link https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Functions_and_function_scope/arguments}
                     * @type {Array}
                     */
                    var args = Array.prototype.slice.call(arguments, 1);

                    if (angular.isDefined(callback))
                    {
                        scope.$evalAsync(function ()
                        {
                            if (angular.isFunction(callback))
                            {
                                callback(args);
                            } else
                            {
                                throw new Error('ui-ace use a function as callback.');
                            }
                        });
                    }
                };



                /**
                 * Listener factory. Until now only change listeners can be created.
                 * @type object
                 */
                var listenerFactory = {
                    /**
                     * Creates a change listener which propagates the change event
                     * and the editor session to the callback from the user option
                     * onChange. It might be exchanged during runtime, if this
                     * happens the old listener will be unbound.
                     *
                     * @param callback callback function defined in the user options
                     * @see onChangeListener
                     */
                    onChange: function (callback)
                    {
                        return function (e)
                        {
                            var newValue = editor.getValue();
                            angularHelper.safeApply(scope, function ()
                            {
                                scope.model = newValue;
                            });
                            executeUserCallback(callback, e, editor);
                        };
                    },
                    /**
                     * Creates a blur listener which propagates the editor session
                     * to the callback from the user option onBlur. It might be
                     * exchanged during runtime, if this happens the old listener
                     * will be unbound.
                     *
                     * @param callback callback function defined in the user options
                     * @see onBlurListener
                     */
                    onBlur: function (callback)
                    {
                        return function ()
                        {
                            executeUserCallback(callback, editor);
                        };
                    }
                };

                attr.$observe('readonly', function (value)
                {
                    monaco.setReadOnly(!!value || value === '');
                });

                // Value Blind
                if (scope.model)
                {
                    editor.setValue(scope.model);
                }

                // Listen for option updates
                const updateOptions = function (current, previous)
                {
                    if (current === previous)
                    {
                        return;
                    }

                    opts = angular.extend({}, options, scope.umbMonacoEditor);

                    opts.callbacks = [opts.onLoad];
                    if (opts.onLoad !== options.onLoad)
                    {
                        // also call the global onLoad handler
                        opts.callbacks.unshift(options.onLoad);
                    }

                    if (opts.autoFocus === true)
                    {
                        editor.focus();
                    }

                    // EVENTS

                    // bind new change listener
                    onChangeListener = listenerFactory.onChange(opts.onChange);
                    editor.onDidChangeModelContent(onChangeListener);

                    // bind new blur listener
                    onBlurListener = listenerFactory.onBlur(opts.onBlur);
                    editor.onDidBlurEditorText(onBlurListener);

                    setOptions(editor, opts);
                };

                scope.$watch(scope.umbMonacoEditor, updateOptions, /* deep watch */ true);

                // set the options here, even if we try to watch later, if this
                // line is missing things go wrong (and the tests will also fail)
                updateOptions(options);

                el.on('$destroy', function ()
                {
                    editor.dispose();
                });

                scope.$watch(function ()
                {
                    return [el[0].offsetWidth, el[0].offsetHeight];
                }, function ()
                    {
                        editor.layout();
                    }, true);

                window.onresize = function ()
                {
                    editor.layout();
                };
            }

        }

        var directive = {
            restrict: 'EA',
            scope: {
                "umbMonacoEditor": "=",
                "model": "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives')
        .constant('umbMonacoEditorConfig', {})
        .directive('umbMonacoEditor', MonacoEditorDirective);

})();
