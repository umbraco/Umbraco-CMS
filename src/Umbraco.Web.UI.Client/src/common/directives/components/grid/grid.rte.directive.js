angular.module("umbraco.directives")
    .directive('gridRte', function (tinyMceService, stylesheetResource, angularHelper, assetsService, $q, $timeout, eventsService) {
        return {
            scope: {
                uniqueId: '=',
                value: '=',
                onClick: '&',
                onFocus: '&',
                onBlur: '&',
                configuration:"=",
                onMediaPickerClick: "=",
                onEmbedClick: "=",
                onMacroPickerClick: "=",
                onLinkPickerClick: "="
            },
            template: "<textarea ng-model=\"value\" rows=\"10\" class=\"mceNoEditor\" style=\"overflow:hidden\" id=\"{{uniqueId}}\"></textarea>",
            replace: true,
            link: function (scope, element, attrs) {

                var initTiny = function () {

                    //we always fetch the default one, and then override parts with our own
                    tinyMceService.configuration().then(function (tinyMceConfig) {



                        //config value from general tinymce.config file
                        var validElements = tinyMceConfig.validElements;
                        var fallbackStyles = [{title: "Page header", block: "h2"}, {title: "Section header", block: "h3"}, {title: "Paragraph header", block: "h4"}, {title: "Normal", block: "p"}, {title: "Quote", block: "blockquote"}, {title: "Code", block: "code"}];

                        //These are absolutely required in order for the macros to render inline
                        //we put these as extended elements because they get merged on top of the normal allowed elements by tiny mce
                        var extendedValidElements = "@[id|class|style],-div[id|dir|class|align|style],ins[datetime|cite],-ul[class|style],-li[class|style],-h1[id|dir|class|align|style],-h2[id|dir|class|align|style],-h3[id|dir|class|align|style],-h4[id|dir|class|align|style],-h5[id|dir|class|align|style],-h6[id|style|dir|class|align],span[id|class|style]";

                        var invalidElements = tinyMceConfig.inValidElements;
                        var plugins = _.map(tinyMceConfig.plugins, function (plugin) {
                            if (plugin.useOnFrontend) {
                                return plugin.name;
                            }
                        }).join(" ") + " autoresize";

                        //config value on the data type
                        var toolbar = ["code", "styleselect", "bold", "italic", "alignleft", "aligncenter", "alignright", "bullist", "numlist", "link", "umbmediapicker", "umbembeddialog"].join(" | ");
                        var stylesheets = [];

                        var styleFormats = [];
                        var await = [];

                        //queue file loading
                        if (typeof (tinymce) === "undefined") {
                                await.push(assetsService.loadJs("lib/tinymce/tinymce.min.js", scope));
                        }


                        if(scope.configuration && scope.configuration.toolbar){
                            toolbar = scope.configuration.toolbar.join(' | ');
                        }


                        if(scope.configuration && scope.configuration.stylesheets){
                            angular.forEach(scope.configuration.stylesheets, function(stylesheet, key){

                                    stylesheets.push(Umbraco.Sys.ServerVariables.umbracoSettings.cssPath + "/" + stylesheet + ".css");
                                    await.push(stylesheetResource.getRulesByName(stylesheet).then(function (rules) {
                                        angular.forEach(rules, function (rule) {
                                          var r = {};
                                          var split = "";
                                          r.title = rule.name;
                                          if (rule.selector[0] === ".") {
                                              r.inline = "span";
                                              r.classes = rule.selector.substring(1);
                                          }else if (rule.selector[0] === "#") {
                                              //Even though this will render in the style drop down, it will not actually be applied
                                              // to the elements, don't think TinyMCE even supports this and it doesn't really make much sense
                                              // since only one element can have one id.
                                              r.inline = "span";
                                              r.attributes = { id: rule.selector.substring(1) };
                                          }else if (rule.selector[0] !== "." && rule.selector.indexOf(".") > -1) {
                                              split = rule.selector.split(".");
                                              r.block = split[0];
                                              r.classes = rule.selector.substring(rule.selector.indexOf(".") + 1).replace(".", " ");
                                          }else if (rule.selector[0] !== "#" && rule.selector.indexOf("#") > -1) {
                                              split = rule.selector.split("#");
                                              r.block = split[0];
                                              r.classes = rule.selector.substring(rule.selector.indexOf("#") + 1);
                                          }else {
                                              r.block = rule.selector;
                                          }
                                          styleFormats.push(r);
                                        });
                                    }));
                            });
                        }else{
                            stylesheets.push("views/propertyeditors/grid/config/grid.default.rtestyles.css");
                            styleFormats = fallbackStyles;
                        }

                        //stores a reference to the editor
                        var tinyMceEditor = null;
                        $q.all(await).then(function () {

                            var uniqueId = scope.uniqueId;

                            //create a baseline Config to exten upon
                            var baseLineConfigObj = {
                                mode: "exact",
                                skin: "umbraco",
                                plugins: plugins,
                                valid_elements: validElements,
                                invalid_elements: invalidElements,
                                extended_valid_elements: extendedValidElements,
                                menubar: false,
                                statusbar: false,
                                relative_urls: false,
                                toolbar: toolbar,
                                content_css: stylesheets,
                                style_formats: styleFormats,
                                autoresize_bottom_margin: 0,
                                //see http://archive.tinymce.com/wiki.php/Configuration:cache_suffix
                                cache_suffix: "?umb__rnd=" + Umbraco.Sys.ServerVariables.application.cacheBuster
                            };


                            if (tinyMceConfig.customConfig) {

                                //if there is some custom config, we need to see if the string value of each item might actually be json and if so, we need to
                                // convert it to json instead of having it as a string since this is what tinymce requires
                                for (var i in tinyMceConfig.customConfig) {
                                    var val = tinyMceConfig.customConfig[i];
                                    if (val) {
                                        val = val.toString().trim();
                                        if (val.detectIsJson()) {
                                            try {
                                                tinyMceConfig.customConfig[i] = JSON.parse(val);
                                                //now we need to check if this custom config key is defined in our baseline, if it is we don't want to
                                                //overwrite the baseline config item if it is an array, we want to concat the items in the array, otherwise
                                                //if it's an object it will overwrite the baseline
                                                if (angular.isArray(baseLineConfigObj[i]) && angular.isArray(tinyMceConfig.customConfig[i])) {
                                                    //concat it and below this concat'd array will overwrite the baseline in angular.extend
                                                    tinyMceConfig.customConfig[i] = baseLineConfigObj[i].concat(tinyMceConfig.customConfig[i]);
                                                }
                                            }
                                            catch (e) {
                                                //cannot parse, we'll just leave it
                                            }
                                        }
                                    }
                                    if (val === "true") {
                                        tinyMceConfig.customConfig[i] = true;
                                    }
                                    if (val === "false") {
                                        tinyMceConfig.customConfig[i] = false;
                                    }
                                }

                                angular.extend(baseLineConfigObj, tinyMceConfig.customConfig);
                            }

                            //set all the things that user configs should not be able to override
                            baseLineConfigObj.elements = uniqueId;
                            baseLineConfigObj.setup = function (editor) {

                                //set the reference
                                tinyMceEditor = editor;


                                //enable browser based spell checking
                                editor.on('init', function (e) {

                                    editor.getBody().setAttribute('spellcheck', true);

                                    //force overflow to hidden to prevent no needed scroll
                                    editor.getBody().style.overflow = "hidden";

                                    $timeout(function(){
                                        if(scope.value === null){
                                            editor.focus();
                                        }
                                    }, 400);

                                });

                                // pin toolbar to top of screen if we have focus and it scrolls off the screen
                                var pinToolbar = function () {

                                    var _toolbar = $(editor.editorContainer).find(".mce-toolbar");
                                    var toolbarHeight = _toolbar.height();

                                    var _tinyMce = $(editor.editorContainer);
                                    var tinyMceRect = _tinyMce[0].getBoundingClientRect();
                                    var tinyMceTop = tinyMceRect.top;
                                    var tinyMceBottom = tinyMceRect.bottom;
                                    var tinyMceWidth = tinyMceRect.width;

                                    var _tinyMceEditArea = _tinyMce.find(".mce-edit-area");

                                    // set padding in top of mce so the content does not "jump" up
                                    _tinyMceEditArea.css("padding-top", toolbarHeight);

                                    if (tinyMceTop < 160 && ((160 + toolbarHeight) < tinyMceBottom)) {
                                        _toolbar
                                            .css("visibility", "visible")
                                            .css("position", "fixed")
                                            .css("top", "160px")
                                            .css("margin-top", "0")
                                            .css("width", tinyMceWidth);
                                    } else {
                                        _toolbar
                                            .css("visibility", "visible")
                                            .css("position", "absolute")
                                            .css("top", "auto")
                                            .css("margin-top", "0")
                                            .css("width", tinyMceWidth);
                                    }
                                    
                                };

                                // unpin toolbar to top of screen
                                var unpinToolbar = function() {

                                    var _toolbar = $(editor.editorContainer).find(".mce-toolbar");
                                    var _tinyMce = $(editor.editorContainer);
                                    var _tinyMceEditArea = _tinyMce.find(".mce-edit-area");

                                    // reset padding in top of mce so the content does not "jump" up
                                    _tinyMceEditArea.css("padding-top", "0");
                                        
                                    _toolbar.css("position", "static");

                                };

                                //when we leave the editor (maybe)
                                editor.on('blur', function (e) {
                                    editor.save();
                                    angularHelper.safeApply(scope, function () {
                                        scope.value = editor.getContent();

                                        var _toolbar = $(editor.editorContainer)
                                             .find(".mce-toolbar");

                                        if(scope.onBlur){
                                            scope.onBlur();
                                        }

                                        unpinToolbar();
                                        $('.umb-panel-body').off('scroll', pinToolbar);

                                    });
                                });

                                // Focus on editor
                                editor.on('focus', function (e) {
                                    angularHelper.safeApply(scope, function () {

                                        if(scope.onFocus){
                                            scope.onFocus();
                                        }

                                        pinToolbar();
                                        $('.umb-panel-body').on('scroll', pinToolbar);

                                    });
                                });

                                // Click on editor
                                editor.on('click', function (e) {
                                    angularHelper.safeApply(scope, function () {

                                        if(scope.onClick){
                                            scope.onClick();
                                        }

                                        pinToolbar();
                                        $('.umb-panel-body').on('scroll', pinToolbar);

                                    });
                                });

                                //when buttons modify content
                                editor.on('ExecCommand', function (e) {
                                    editor.save();
                                    angularHelper.safeApply(scope, function () {
                                        scope.value = editor.getContent();
                                    });
                                });

                                // Update model on keypress
                                editor.on('KeyUp', function (e) {
                                    editor.save();
                                    angularHelper.safeApply(scope, function () {
                                        scope.value = editor.getContent();
                                    });
                                });

                                // Update model on change, i.e. copy/pasted text, plugins altering content
                                editor.on('SetContent', function (e) {
                                    if (!e.initial) {
                                        editor.save();
                                        angularHelper.safeApply(scope, function () {
                                            scope.value = editor.getContent();
                                        });
                                    }
                                });

                                editor.on('ObjectResized', function (e) {
                                    var qs = "?width=" + e.width + "&height=" + e.height;
                                    var srcAttr = $(e.target).attr("src");
                                    var path = srcAttr.split("?")[0];
                                    $(e.target).attr("data-mce-src", path + qs);
                                });

                                //Create the insert link plugin
                                tinyMceService.createLinkPicker(editor, scope, function(currentTarget, anchorElement){
                                    if(scope.onLinkPickerClick) {
                                        scope.onLinkPickerClick(editor, currentTarget, anchorElement);
                                    }
                                });

                                //Create the insert media plugin
                                tinyMceService.createMediaPicker(editor, scope, function(currentTarget, userData){
                                    if(scope.onMediaPickerClick) {
                                        scope.onMediaPickerClick(editor, currentTarget, userData);
                                    }
                                });

                                //Create the embedded plugin
                                tinyMceService.createInsertEmbeddedMedia(editor, scope, function(){
                                    if(scope.onEmbedClick) {
                                        scope.onEmbedClick(editor);
                                    }
                                });

                                //Create the insert macro plugin
                                tinyMceService.createInsertMacro(editor, scope, function(dialogData){
                                    if(scope.onMacroPickerClick) {
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
                                    tinymce.init(baseLineConfigObj);
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
								if (tinyMceEditor !== undefined && tinyMceEditor != null) {
									tinyMceEditor.destroy()
								}
                            });

                        });

                    });

                };

                initTiny();

            }
        };
    });
