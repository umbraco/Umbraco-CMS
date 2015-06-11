angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.RTEController",
    function ($rootScope, $scope, $q, dialogService, $log, imageHelper, assetsService, $timeout, tinyMceService, angularHelper, stylesheetResource) {
        
        $scope.isLoading = true;

        //To id the html textarea we need to use the datetime ticks because we can have multiple rte's per a single property alias
        // because now we have to support having 2x (maybe more at some stage) content editors being displayed at once. This is because
        // we have this mini content editor panel that can be launched with MNTP.
        var d = new Date();
        var n = d.getTime();
        $scope.textAreaHtmlId = $scope.model.alias + "_" + n + "_rte";

        var alreadyDirty = false;
        function syncContent(editor){
            editor.save();
            angularHelper.safeApply($scope, function () {
                $scope.model.value = editor.getContent();
            });

            if (!alreadyDirty) {
                //make the form dirty manually so that the track changes works, setting our model doesn't trigger
                // the angular bits because tinymce replaces the textarea.
                var currForm = angularHelper.getCurrentForm($scope);
                currForm.$setDirty();
                alreadyDirty = true;
            }
        }

        tinyMceService.configuration().then(function (tinyMceConfig) {

            //config value from general tinymce.config file
            var validElements = tinyMceConfig.validElements;

            //These are absolutely required in order for the macros to render inline
            //we put these as extended elements because they get merged on top of the normal allowed elements by tiny mce
            var extendedValidElements = "@[id|class|style],-div[id|dir|class|align|style],ins[datetime|cite],-ul[class|style],-li[class|style]";

            var invalidElements = tinyMceConfig.inValidElements;
            var plugins = _.map(tinyMceConfig.plugins, function (plugin) {
                if (plugin.useOnFrontend) {
                    return plugin.name;
                }
            }).join(" ");

            var editorConfig = $scope.model.config.editor;
            if (!editorConfig || angular.isString(editorConfig)) {
                editorConfig = tinyMceService.defaultPrevalues();
            }

            //config value on the data type
            var toolbar = editorConfig.toolbar.join(" | ");
            var stylesheets = [];
            var styleFormats = [];
            var await = [];
            if (!editorConfig.maxImageSize && editorConfig.maxImageSize != 0) {
                editorConfig.maxImageSize = tinyMceService.defaultPrevalues().maxImageSize;
            }

            //queue file loading
            if (typeof tinymce === "undefined") { // Don't reload tinymce if already loaded
                await.push(assetsService.loadJs("lib/tinymce/tinymce.min.js", $scope));
            }

            //queue rules loading
            angular.forEach(editorConfig.stylesheets, function (val, key) {
                stylesheets.push("../css/" + val + ".css?" + new Date().getTime());

                await.push(stylesheetResource.getRulesByName(val).then(function (rules) {
                    angular.forEach(rules, function (rule) {
                        var r = {};
                        r.title = rule.name;
                        if (rule.selector[0] == ".") {
                            r.inline = "span";
                            r.classes = rule.selector.substring(1);
                        }
                        else if (rule.selector[0] == "#") {
                            r.inline = "span";
                            r.attributes = { id: rule.selector.substring(1) };
                        }
                        else if (rule.selector[0] != "." && rule.selector.indexOf(".") > -1) {
                            var split = rule.selector.split(".");
                            r.block = split[0];
                            r.classes = rule.selector.substring(rule.selector.indexOf(".") + 1).replace(".", " ");
                        }
                        else if (rule.selector[0] != "#" && rule.selector.indexOf("#") > -1) {
                            var split = rule.selector.split("#");
                            r.block = split[0];
                            r.classes = rule.selector.substring(rule.selector.indexOf("#") + 1);
                        }
                        else {
                            r.block = rule.selector;
                        }

                        styleFormats.push(r);
                    });
                }));
            });


            //stores a reference to the editor
            var tinyMceEditor = null;

            //wait for queue to end
            $q.all(await).then(function () {

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
                    height: editorConfig.dimensions.height,
                    width: editorConfig.dimensions.width,
                    maxImageSize: editorConfig.maxImageSize,
                    toolbar: toolbar,
                    content_css: stylesheets.join(','),
                    relative_urls: false,
                    style_formats: styleFormats
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
                    }

                    angular.extend(baseLineConfigObj, tinyMceConfig.customConfig);
                }

                //set all the things that user configs should not be able to override
                baseLineConfigObj.elements = $scope.textAreaHtmlId; //this is the exact textarea id to replace!
                baseLineConfigObj.setup = function (editor) {

                    //set the reference
                    tinyMceEditor = editor;

                    //enable browser based spell checking
                    editor.on('init', function (e) {
                        editor.getBody().setAttribute('spellcheck', true);
                    });

                    //We need to listen on multiple things here because of the nature of tinymce, it doesn't 
                    //fire events when you think!
                    //The change event doesn't fire when content changes, only when cursor points are changed and undo points
                    //are created. the blur event doesn't fire if you insert content into the editor with a button and then 
                    //press save. 
                    //We have a couple of options, one is to do a set timeout and check for isDirty on the editor, or we can 
                    //listen to both change and blur and also on our own 'saving' event. I think this will be best because a 
                    //timer might end up using unwanted cpu and we'd still have to listen to our saving event in case they clicked
                    //save before the timeout elapsed.

                    //TODO: We need to re-enable something like this to ensure the track changes is working with tinymce
                    // so we can detect if the form is dirty or not, Per has some better events to use as this one triggers
                    // even if you just enter/exit with mouse cursuor which doesn't really mean it's changed.
                    // see: http://issues.umbraco.org/issue/U4-4485
                    //var alreadyDirty = false;
                    //editor.on('change', function (e) {
                    //    angularHelper.safeApply($scope, function () {
                    //        $scope.model.value = editor.getContent();

                    //        if (!alreadyDirty) {
                    //            //make the form dirty manually so that the track changes works, setting our model doesn't trigger
                    //            // the angular bits because tinymce replaces the textarea.
                    //            var currForm = angularHelper.getCurrentForm($scope);
                    //            currForm.$setDirty();
                    //            alreadyDirty = true;
                    //        }
                            
                    //    });
                    //});

                    //when we leave the editor (maybe)
                    editor.on('blur', function (e) {
                        editor.save();
                        angularHelper.safeApply($scope, function () {
                            $scope.model.value = editor.getContent();
                        });
                    });

                    //when buttons modify content
                    editor.on('ExecCommand', function (e) {
                        syncContent(editor);
                    });

                    // Update model on keypress
                    editor.on('KeyUp', function (e) {
                        syncContent(editor);
                    });

                    // Update model on change, i.e. copy/pasted text, plugins altering content
                    editor.on('SetContent', function (e) {
                        if (!e.initial) {
                            syncContent(editor);
                        }
                    });


                    editor.on('ObjectResized', function (e) {
                        var qs = "?width=" + e.width + "px&height=" + e.height + "px";
                        var srcAttr = $(e.target).attr("src");
                        var path = srcAttr.split("?")[0];
                        $(e.target).attr("data-mce-src", path + qs);

                        syncContent(editor);
                    });


                    //Create the insert media plugin
                    tinyMceService.createMediaPicker(editor, $scope);

                    //Create the embedded plugin
                    tinyMceService.createInsertEmbeddedMedia(editor, $scope);

                    //Create the insert macro plugin
                    tinyMceService.createInsertMacro(editor, $scope);
                };




                /** Loads in the editor */
                function loadTinyMce() {

                    //we need to add a timeout here, to force a redraw so TinyMCE can find
                    //the elements needed
                    $timeout(function () {
                        tinymce.DOM.events.domLoaded = true;
                        tinymce.init(baseLineConfigObj);

                        $scope.isLoading = false;

                    }, 200, false);
                }




                loadTinyMce();

                //here we declare a special method which will be called whenever the value has changed from the server
                //this is instead of doing a watch on the model.value = faster
                $scope.model.onValueChanged = function (newVal, oldVal) {
                    //update the display val again if it has changed from the server;
                    tinyMceEditor.setContent(newVal, { format: 'raw' });
                    //we need to manually fire this event since it is only ever fired based on loading from the DOM, this
                    // is required for our plugins listening to this event to execute
                    tinyMceEditor.fire('LoadContent', null);
                };

                //listen for formSubmitting event (the result is callback used to remove the event subscription)
                var unsubscribe = $scope.$on("formSubmitting", function () {
                    //TODO: Here we should parse out the macro rendered content so we can save on a lot of bytes in data xfer
                    // we do parse it out on the server side but would be nice to do that on the client side before as well.
                    $scope.model.value = tinyMceEditor.getContent();
                });

                //when the element is disposed we need to unsubscribe!
                // NOTE: this is very important otherwise if this is part of a modal, the listener still exists because the dom 
                // element might still be there even after the modal has been hidden.
                $scope.$on('$destroy', function () {
                    unsubscribe();
                });
            });
        });

    });
