angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.RTEController",
    function ($rootScope, $scope, $q, $locale, dialogService, $log, imageHelper, assetsService, $timeout, tinyMceService, angularHelper, stylesheetResource, macroService, editorState) {

        $scope.isLoading = true;

        //To id the html textarea we need to use the datetime ticks because we can have multiple rte's per a single property alias
        // because now we have to support having 2x (maybe more at some stage) content editors being displayed at once. This is because
        // we have this mini content editor panel that can be launched with MNTP.
        var d = new Date();
        var n = d.getTime();
        $scope.textAreaHtmlId = $scope.model.alias + "_" + n + "_rte";
        
        function syncContent(editor){
            angularHelper.safeApply($scope, function () {
                $scope.model.value = editor.getContent();
            });
        }

        tinyMceService.configuration().then(function (tinyMceConfig) {

            //config value from general tinymce.config file
            var validElements = tinyMceConfig.validElements;

            //These are absolutely required in order for the macros to render inline
            //we put these as extended elements because they get merged on top of the normal allowed elements by tiny mce
            var extendedValidElements = "@[id|class|style],-div[id|dir|class|align|style],ins[datetime|cite],-ul[class|style],-li[class|style],span[id|class|style]";

            var invalidElements = tinyMceConfig.inValidElements;
            var plugins = _.map(tinyMceConfig.plugins,
                function(plugin) {
                    return plugin.name;
                });

            var editorConfig = $scope.model.config.editor;
            if (!editorConfig || angular.isString(editorConfig)) {
                editorConfig = tinyMceService.defaultPrevalues();
            }

            //config value on the data type
            var toolbar = editorConfig.toolbar;
            //TODO: get this from the config
            var allowedSelectionToolbar = ["bold", "italic", "alignleft", "aligncenter", "alignright", "bullist", "numlist", "outdent", "indent", "link"];
            var allowedInsertToolbar = ["link", "image", "umbmediapicker", "umbembeddialog", "umbmacro", "bullist", "numlist"];
            var insertToolbar = _.filter(editorConfig.toolbar, function (t) {
                return allowedInsertToolbar.indexOf(t) !== -1;
            }).join(" | ");
            var selectionToolbar = _.filter(editorConfig.toolbar, function(t) {
                return allowedSelectionToolbar.indexOf(t) !== -1;
            }).join(" | ");

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
                stylesheets.push(Umbraco.Sys.ServerVariables.umbracoSettings.cssPath + "/" + val + ".css?" + new Date().getTime());
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

            // these languages are available for localization
            var availableLanguages = [
                'da',
                'de',
                'en',
                'en_us',
                'fi',
                'fr',
                'he',
                'it',
                'ja',
                'nl',
                'no',
                'pl',
                'pt',
                'ru',
                'sv',
                'zh'
            ];

            //define fallback language
            var language = 'en_us';
            //get locale from angular and match tinymce format. Angular localization is always in the format of ru-ru, de-de, en-gb, etc.
            //wheras tinymce is in the format of ru, de, en, en_us, etc.
            var localeId = $locale.id.replace('-', '_');
            //try matching the language using full locale format
            var languageMatch = _.find(availableLanguages, function(o) { return o === localeId; });
            //if no matches, try matching using only the language
            if (languageMatch === undefined) {
                var localeParts = localeId.split('_');
                languageMatch = _.find(availableLanguages, function(o) { return o === localeParts[0]; });
            }
            //if a match was found - set the language
            if (languageMatch !== undefined) {
                language = languageMatch;
            }

            //wait for queue to end
            $q.all(await).then(function () {
                
                //create a baseline Config to exten upon
                var baseLineConfigObj = {
                    theme: "inlite",
                    inline: true,
                    plugins: plugins,
                    valid_elements: validElements,
                    invalid_elements: invalidElements,
                    extended_valid_elements: extendedValidElements,
                    menubar: false,
                    statusbar: false,
                    relative_urls: false,
                    height: editorConfig.dimensions.height,
                    width: editorConfig.dimensions.width,
                    maxImageSize: editorConfig.maxImageSize,
                    insert_toolbar: insertToolbar,
                    selection_toolbar: selectionToolbar,
                    content_css: stylesheets,
                    style_formats: styleFormats,
                    language: language,
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
                            if (val === "true") {
                                tinyMceConfig.customConfig[i] = true;
                            }
                            if (val === "false") {
                                tinyMceConfig.customConfig[i] = false;
                            }
                        }
                    }

                    angular.extend(baseLineConfigObj, tinyMceConfig.customConfig);
                }

                //set all the things that user configs should not be able to override
                //baseLineConfigObj.elements = $scope.textAreaHtmlId; //this is the exact textarea id to replace!
                baseLineConfigObj.selector = "#" + $scope.textAreaHtmlId;
                baseLineConfigObj.setup = function (editor) {

                    //set the reference
                    tinyMceEditor = editor;

                    //set the value and enable browser based spell checking
                    editor.on('init', function (e) {
                        editor.setContent($scope.model.value);
                        editor.getBody().setAttribute('spellcheck', true);
                    });
                    
                    editor.on('Dirty', function (e) {
                        //make the form dirty manually so that the track changes works, setting our model doesn't trigger
                        // the angular bits because tinymce replaces the textarea.
                        var currForm = angularHelper.getCurrentForm($scope);
                        currForm.$setDirty();
                    });

                    editor.on('Change', function (e) {
                        console.log("Change...");
                        syncContent(editor);
                    });

                    editor.on('ObjectResized', function (e) {
                        console.log("ObjectResized...");

                        var qs = "?width=" + e.width + "&height=" + e.height + "&mode=max";
                        var srcAttr = $(e.target).attr("src");
                        var path = srcAttr.split("?")[0];
                        $(e.target).attr("data-mce-src", path + qs);

                        syncContent(editor);
                    });
					
                    tinyMceService.createLinkPicker(editor, $scope, function(currentTarget, anchorElement) {
                        $scope.linkPickerOverlay = {
                            view: "linkpicker",
                            currentTarget: currentTarget,
							anchors: tinyMceService.getAnchorNames(JSON.stringify(editorState.current.properties)),
                            show: true,
                            submit: function(model) {
                                tinyMceService.insertLinkInEditor(editor, model.target, anchorElement);
                                $scope.linkPickerOverlay.show = false;
                                $scope.linkPickerOverlay = null;
                            }
                        };
                    });

                    //Create the insert media plugin
                    tinyMceService.createMediaPicker(editor, $scope, function(currentTarget, userData){

                        $scope.mediaPickerOverlay = {
                            currentTarget: currentTarget,
                            onlyImages: true,
                            showDetails: true,
                            disableFolderSelect: true,
                            startNodeId: userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0],
                            startNodeIsVirtual: userData.startMediaIds.length !== 1,
                            view: "mediapicker",
                            show: true,
                            submit: function(model) {
                                tinyMceService.insertMediaInEditor(editor, model.selectedImages[0]);
                                $scope.mediaPickerOverlay.show = false;
                                $scope.mediaPickerOverlay = null;
                            }
                        };

                    });
                    
                    //Create the embedded plugin
                    tinyMceService.createInsertEmbeddedMedia(editor, $scope, function() {

                      $scope.embedOverlay = {
                          view: "embed",
                          show: true,
                          submit: function(model) {
                              tinyMceService.insertEmbeddedMediaInEditor(editor, model.embed.preview);
                              $scope.embedOverlay.show = false;
                              $scope.embedOverlay = null;
                          }
                      };

                    });


                    //Create the insert macro plugin
                    tinyMceService.createInsertMacro(editor, $scope, function(dialogData) {

                        $scope.macroPickerOverlay = {
                            view: "macropicker",
                            dialogData: dialogData,
                            show: true,
                            submit: function(model) {
                                var macroObject = macroService.collectValueData(model.selectedMacro, model.macroParams, dialogData.renderingEngine);
                                tinyMceService.insertMacroInEditor(editor, macroObject, $scope);
                                $scope.macroPickerOverlay.show = false;
                                $scope.macroPickerOverlay = null;
                            }
                        };

                    });
                };
                
                /** Loads in the editor */
                function loadTinyMce() {

                    //we need to add a timeout here, to force a redraw so TinyMCE can find
                    //the elements needed
                    $timeout(function () {
                        tinymce.DOM.events.domLoaded = true;
                        tinymce.init(baseLineConfigObj);

                        $scope.isLoading = false;

                    }, 200);
                }




                loadTinyMce();

                //here we declare a special method which will be called whenever the value has changed from the server
                //this is instead of doing a watch on the model.value = faster
                $scope.model.onValueChanged = function (newVal, oldVal) {
                    //update the display val again if it has changed from the server;
                    //uses an empty string in the editor when the value is null
                    tinyMceEditor.setContent(newVal || "", { format: 'raw' });
                    //we need to manually fire this event since it is only ever fired based on loading from the DOM, this
                    // is required for our plugins listening to this event to execute
                    tinyMceEditor.fire('LoadContent', null);
                };

                //listen for formSubmitting event (the result is callback used to remove the event subscription)
                var unsubscribe = $scope.$on("formSubmitting", function () {
                    //TODO: Here we should parse out the macro rendered content so we can save on a lot of bytes in data xfer
                    // we do parse it out on the server side but would be nice to do that on the client side before as well.
                    $scope.model.value = tinyMceEditor ? tinyMceEditor.getContent() : null;
                });

                //when the element is disposed we need to unsubscribe!
                // NOTE: this is very important otherwise if this is part of a modal, the listener still exists because the dom
                // element might still be there even after the modal has been hidden.
                $scope.$on('$destroy', function () {
                    unsubscribe();
					if (tinyMceEditor !== undefined && tinyMceEditor != null) {
						tinyMceEditor.destroy()
					}
                });
            });
        });

    });
