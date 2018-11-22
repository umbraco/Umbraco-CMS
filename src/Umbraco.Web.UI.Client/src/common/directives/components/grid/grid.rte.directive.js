angular.module("umbraco.directives")
    .directive('gridRte', function (tinyMceService, angularHelper, assetsService, $q, $timeout, eventsService) {
        return {
            scope: {
                uniqueId: '=',
                value: '=',
                onClick: '&',
                onFocus: '&',
                onBlur: '&',
                configuration: "=",
                onMediaPickerClick: "=",
                onEmbedClick: "=",
                onMacroPickerClick: "=",
                onLinkPickerClick: "="
            },
            templateUrl: 'views/components/grid/grid-rte.html',
            replace: true,
            link: function (scope, element, attrs) {

                //TODO: A lot of the code below should be shared between the grid rte and the normal rte

                var promises = [];

                //queue file loading
                if (typeof (tinymce) === "undefined") {
                    promises.push(assetsService.loadJs("lib/tinymce/tinymce.min.js", scope));
                }

                var toolbar = ["code", "styleselect", "bold", "italic", "alignleft", "aligncenter", "alignright", "bullist", "numlist", "link", "umbmediapicker", "umbembeddialog"];
                if (scope.configuration && scope.configuration.toolbar) {
                    toolbar = scope.configuration.toolbar;
                }
                
                //stores a reference to the editor
                var tinyMceEditor = null;

                promises.push(tinyMceService.getTinyMceEditorConfig({
                    htmlId: scope.uniqueId,
                    stylesheets: scope.configuration ? scope.configuration.stylesheets : null,
                    toolbar: toolbar
                }));

                $q.all(promises).then(function (result) {

                    var tinyMceEditorConfig = result[promises.length - 1];

                    tinyMceEditorConfig.setup = function (editor) {

                        //set the reference
                        tinyMceEditor = editor;

                        //enable browser based spell checking
                        editor.on('init', function (e) {

                            if (!scope.value) {
                                scope.value = "";
                            }
                            editor.setContent(scope.value);

                            editor.getBody().setAttribute('spellcheck', true);

                            //force overflow to hidden to prevent no needed scroll
                            editor.getBody().style.overflow = "hidden";

                            $timeout(function () {
                                if (scope.value === null) {
                                    editor.focus();
                                }
                            }, 400);

                        });

                        // pin toolbar to top of screen if we have focus and it scrolls off the screen
                        function pinToolbar() {
                            tinyMceService.pinToolbar(editor);
                        }

                        // unpin toolbar to top of screen
                        function unpinToolbar() {
                            tinyMceService.unpinToolbar(editor);
                        } 

                        //when we leave the editor (maybe)
                        editor.on('blur', function (e) {
                            angularHelper.safeApply(scope, function () {
                                scope.value = editor.getContent();

                                if (scope.onBlur) {
                                    scope.onBlur();
                                }

                                unpinToolbar();
                                $('.umb-panel-body').off('scroll', pinToolbar);
                            });
                        });

                        // Focus on editor
                        editor.on('focus', function (e) {
                            angularHelper.safeApply(scope, function () {

                                if (scope.onFocus) {
                                    scope.onFocus();
                                }

                                pinToolbar();
                                $('.umb-panel-body').on('scroll', pinToolbar);
                            });
                        });

                        // Click on editor
                        editor.on('click', function (e) {
                            angularHelper.safeApply(scope, function () {

                                if (scope.onClick) {
                                    scope.onClick();
                                }

                                pinToolbar();
                                $('.umb-panel-body').on('scroll', pinToolbar);

                            });
                        });

                        // Update model on change, i.e. copy/pasted text, plugins altering content
                        editor.on('Change', function (e) {
                            angularHelper.safeApply(scope, function () {
                                scope.value = editor.getContent();
                            });
                        });

                        editor.on('ObjectResized', function (e) {
                            var qs = "?width=" + e.width + "&height=" + e.height;
                            var srcAttr = $(e.target).attr("src");
                            var path = srcAttr.split("?")[0];
                            $(e.target).attr("data-mce-src", path + qs);
                        });

                        //Create the insert link plugin
                        tinyMceService.createLinkPicker(editor, scope, function (currentTarget, anchorElement) {
                            if (scope.onLinkPickerClick) {
                                scope.onLinkPickerClick(editor, currentTarget, anchorElement);
                            }
                        });

                        //Create the insert media plugin
                        tinyMceService.createMediaPicker(editor, scope, function (currentTarget, userData) {
                            if (scope.onMediaPickerClick) {
                                scope.onMediaPickerClick(editor, currentTarget, userData);
                            }
                        });

                        //Create the embedded plugin
                        tinyMceService.createInsertEmbeddedMedia(editor, scope, function () {
                            if (scope.onEmbedClick) {
                                scope.onEmbedClick(editor);
                            }
                        });

                        //Create the insert macro plugin
                        tinyMceService.createInsertMacro(editor, scope, function (dialogData) {
                            if (scope.onMacroPickerClick) {
                                scope.onMacroPickerClick(editor, dialogData);
                            }
                        });

                    };

                    /** Loads in the editor */
                    function loadTinyMce() {

                        //we need to add a timeout here, to force a redraw so TinyMCE can find
                        //the elements needed
                        $timeout(function () {
                            tinymce.DOM.events.domLoaded = true;
                            tinymce.init(tinyMceEditorConfig);
                        }, 150, false);
                    }

                    loadTinyMce();

                    //here we declare a special method which will be called whenever the value has changed from the server
                    //this is instead of doing a watch on the model.value = faster
                    //scope.model.onValueChanged = function (newVal, oldVal) {
                    //    //update the display val again if it has changed from the server;
                    //    tinyMceEditor.setContent(newVal, { format: 'raw' });
                    //    //we need to manually fire this event since it is only ever fired based on loading from the DOM, this
                    //    // is required for our plugins listening to this event to execute
                    //    tinyMceEditor.fire('LoadContent', null);
                    //};

                             
                            var tabShownListener = eventsService.on("app.tabChange", function (e, args) {

                                var tabId = args.id;
                                var myTabId = element.closest(".umb-tab-pane").attr("rel");

                                if (String(tabId) === myTabId) {
                                    //the tab has been shown, trigger the mceAutoResize (as it could have timed out before the tab was shown)
                                    if (tinyMceEditor !== undefined && tinyMceEditor != null) {
                                        tinyMceEditor.execCommand('mceAutoResize', false, null, null);
                                    }
                                }
                                
                            });
                           
                    //listen for formSubmitting event (the result is callback used to remove the event subscription)
                    var formSubmittingListener = scope.$on("formSubmitting", function () {
                        //TODO: Here we should parse out the macro rendered content so we can save on a lot of bytes in data xfer
                        // we do parse it out on the server side but would be nice to do that on the client side before as well.
                        scope.value = tinyMceEditor ? tinyMceEditor.getContent() : null;
                    });

                    //when the element is disposed we need to unsubscribe!
                    // NOTE: this is very important otherwise if this is part of a modal, the listener still exists because the dom
                    // element might still be there even after the modal has been hidden.
                    scope.$on('$destroy', function () {
                        formSubmittingListener();
                        eventsService.unsubscribe(tabShownListener);
                        //ensure we unbind this in case the blur doesn't fire above
                        $('.umb-panel-body').off('scroll', pinToolbar);
                        if (tinyMceEditor !== undefined && tinyMceEditor != null) {
                            tinyMceEditor.destroy()
                        }
                    });

                });

            }
        };
    });
