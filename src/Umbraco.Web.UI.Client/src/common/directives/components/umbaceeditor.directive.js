(function() {
    'use strict';

    function AceEditorDirective(umbAceEditorConfig, assetsService, angularHelper) {

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
        var setOptions = function(acee, session, opts) {

            // sets the ace worker path, if running from concatenated
            // or minified source
            if (Utilities.isDefined(opts.workerPath)) {
                var config = window.ace.require('ace/config');
                config.set('workerPath', opts.workerPath);
            }

            // ace requires loading
            if (Utilities.isDefined(opts.require)) {
                opts.require.forEach(function(n) {
                    window.ace.require(n);
                });
            }

            // Boolean options
            if (Utilities.isDefined(opts.showGutter)) {
                acee.renderer.setShowGutter(opts.showGutter);
            }
            if (Utilities.isDefined(opts.useWrapMode)) {
                session.setUseWrapMode(opts.useWrapMode);
            }
            if (Utilities.isDefined(opts.showInvisibles)) {
                acee.renderer.setShowInvisibles(opts.showInvisibles);
            }
            if (Utilities.isDefined(opts.showIndentGuides)) {
                acee.renderer.setDisplayIndentGuides(opts.showIndentGuides);
            }
            if (Utilities.isDefined(opts.useSoftTabs)) {
                session.setUseSoftTabs(opts.useSoftTabs);
            }
            if (Utilities.isDefined(opts.showPrintMargin)) {
                acee.setShowPrintMargin(opts.showPrintMargin);
            }

            // commands
            if (Utilities.isDefined(opts.disableSearch) && opts.disableSearch) {
                acee.commands.addCommands([{
                    name: 'unfind',
                    bindKey: {
                        win: 'Ctrl-F',
                        mac: 'Command-F'
                    },
                    exec: function() {
                        return false;
                    },
                    readOnly: true
                }]);
            }

            // Basic options
            if (Utilities.isString(opts.theme)) {
                acee.setTheme('ace/theme/' + opts.theme);
            }
            if (Utilities.isString(opts.mode)) {
                session.setMode('ace/mode/' + opts.mode);
            }
            // Advanced options
            if (Utilities.isDefined(opts.firstLineNumber)) {
                if (Utilities.isNumber(opts.firstLineNumber)) {
                    session.setOption('firstLineNumber', opts.firstLineNumber);
                } else if (Utilities.isFunction(opts.firstLineNumber)) {
                    session.setOption('firstLineNumber', opts.firstLineNumber());
                }
            }

            // advanced options
            var key, obj;
            if (Utilities.isDefined(opts.advanced)) {
                for (key in opts.advanced) {
                    // create a javascript object with the key and value
                    obj = {
                        name: key,
                        value: opts.advanced[key]
                    };
                    // try to assign the option to the ace editor
                    acee.setOption(obj.name, obj.value);
                }
            }

            // advanced options for the renderer
            if (Utilities.isDefined(opts.rendererOptions)) {
                for (key in opts.rendererOptions) {
                    // create a javascript object with the key and value
                    obj = {
                        name: key,
                        value: opts.rendererOptions[key]
                    };
                    // try to assign the option to the ace editor
                    acee.renderer.setOption(obj.name, obj.value);
                }
            }

            // onLoad callbacks
            Utilities.forEach(opts.callbacks, cb => {
                if (Utilities.isFunction(cb)) {
                    cb(acee);
                }
            });
        };

        function link(scope, el, attr, ngModel) {

            // Load in ace library
            assetsService.load(['lib/ace-builds/src-min-noconflict/ace.js', 'lib/ace-builds/src-min-noconflict/ext-language_tools.js'], scope).then(function () {
                if (Utilities.isUndefined(window.ace)) {
                    throw new Error('ui-ace need ace to work... (o rly?)');
                } else {
                    // init editor
                    init();
                }
            });

            function init() {

                /**
                 * Corresponds the umbAceEditorConfig ACE configuration.
                 * @type object
                 */
                var options = umbAceEditorConfig.ace || {};

                /**
                 * umbAceEditorConfig merged with user options via json in attribute or data binding
                 * @type object
                 */
                var opts = Utilities.extend({}, options, scope.umbAceEditor);

                //load ace libraries here... 

                /**
                 * ACE editor
                 * @type object
                 */
                var acee = window.ace.edit(el[0]);
                acee.$blockScrolling = Infinity;

                /**
                 * ACE editor session.
                 * @type object
                 * @see [EditSession]{@link https://ace.c9.io/#nav=api&api=edit_session}
                 */
                var session = acee.getSession();

                /**
                 * Reference to a change listener created by the listener factory.
                 * @function
                 * @see listenerFactory.onChange
                 */
                var onChangeListener;

                /**
                 * Reference to a blur listener created by the listener factory.
                 * @function
                 * @see listenerFactory.onBlur
                 */
                var onBlurListener;

                /**
                 * Calls a callback by checking its existing. The argument list
                 * is variable and thus this function is relying on the arguments
                 * object.
                 * @throws {Error} If the callback isn't a function
                 */
                var executeUserCallback = function() {

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

                    if (Utilities.isDefined(callback)) {
                        scope.$evalAsync(function() {
                            if (Utilities.isFunction(callback)) {
                                callback(args);
                            } else {
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
                    onChange: function(callback) {
                        return function(e) {
                            var newValue = session.getValue();
                            angularHelper.safeApply(scope, function () {
                                scope.model = newValue;
                            });
                            executeUserCallback(callback, e, acee);
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
                    onBlur: function(callback) {
                        return function() {
                            executeUserCallback(callback, acee);
                        };
                    }
                };

                attr.$observe('readonly', function(value) {
                    acee.setReadOnly(!!value || value === '');
                });

                // Value Blind
                if(scope.model) {
                    session.setValue(scope.model);
                }

                // Listen for option updates
                var updateOptions = function(current, previous) {
                    if (current === previous) {
                        return;
                    }

                    opts = Utilities.extend({}, options, scope.umbAceEditor);

                    opts.callbacks = [opts.onLoad];
                    if (opts.onLoad !== options.onLoad) {
                        // also call the global onLoad handler
                        opts.callbacks.unshift(options.onLoad);
                    }

                    if (opts.autoFocus === true) {
                        acee.focus();
                    }

                    // EVENTS

                    // unbind old change listener
                    session.removeListener('change', onChangeListener);

                    // bind new change listener
                    onChangeListener = listenerFactory.onChange(opts.onChange);
                    session.on('change', onChangeListener);

                    // unbind old blur listener
                    //session.removeListener('blur', onBlurListener);
                    acee.removeListener('blur', onBlurListener);

                    // bind new blur listener
                    onBlurListener = listenerFactory.onBlur(opts.onBlur);
                    acee.on('blur', onBlurListener);

                    setOptions(acee, session, opts);
                };

                scope.$watch(scope.umbAceEditor, updateOptions, /* deep watch */ true);

                // set the options here, even if we try to watch later, if this
                // line is missing things go wrong (and the tests will also fail)
                updateOptions(options);

                el.on('$destroy', function() {
                    acee.session.$stopWorker();
                    acee.destroy();
                });

                scope.$watch(function() {
                    return [el[0].offsetWidth, el[0].offsetHeight];
                }, function() {
                    acee.resize();
                    acee.renderer.updateFull();
                }, true);

            }

        }

        var directive = {
            restrict: 'EA',
            scope: {
                "umbAceEditor": "=",
                "model": "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives')
        .constant('umbAceEditorConfig', {})
        .directive('umbAceEditor', AceEditorDirective);

})();
