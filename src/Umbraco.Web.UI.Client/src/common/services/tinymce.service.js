/**
 * @ngdoc service
 * @name umbraco.services.tinyMceService
 *
 *
 * @description
 * A service containing all logic for all of the Umbraco TinyMCE plugins
 */
function tinyMceService($rootScope, $q, imageHelper, $locale, $http, $timeout, stylesheetResource, macroResource, macroService, $routeParams, umbRequestHelper, angularHelper, userService, editorService, editorState) {

    //These are absolutely required in order for the macros to render inline
    //we put these as extended elements because they get merged on top of the normal allowed elements by tiny mce
    var extendedValidElements = "@[id|class|style],-div[id|dir|class|align|style],ins[datetime|cite],-ul[class|style],-li[class|style],-h1[id|dir|class|align|style],-h2[id|dir|class|align|style],-h3[id|dir|class|align|style],-h4[id|dir|class|align|style],-h5[id|dir|class|align|style],-h6[id|style|dir|class|align],span[id|class|style]";
    var fallbackStyles = [{ title: "Page header", block: "h2" }, { title: "Section header", block: "h3" }, { title: "Paragraph header", block: "h4" }, { title: "Normal", block: "p" }, { title: "Quote", block: "blockquote" }, { title: "Code", block: "code" }];
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
    var defaultLanguage = 'en_us';

    /**
     * Returns a promise of an object containing the stylesheets and styleFormats collections
     * @param {any} configuredStylesheets
     */
    function getStyles(configuredStylesheets) {

        var stylesheets = [];
        var styleFormats = [];
        var promises = [$q.when(true)]; //a collection of promises, the first one is an empty promise

        //queue rules loading
        if (configuredStylesheets) {
            angular.forEach(configuredStylesheets, function (val, key) {

                stylesheets.push(Umbraco.Sys.ServerVariables.umbracoSettings.cssPath + "/" + val + ".css");

                promises.push(stylesheetResource.getRulesByName(val).then(function (rules) {
                    angular.forEach(rules, function (rule) {
                        var r = {};
                        r.title = rule.name;
                        if (rule.selector[0] == ".") {
                            r.inline = "span";
                            r.classes = rule.selector.substring(1);
                        }
                        else if (rule.selector[0] === "#") {
                            r.inline = "span";
                            r.attributes = { id: rule.selector.substring(1) };
                        }
                        else if (rule.selector[0] !== "." && rule.selector.indexOf(".") > -1) {
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
        }
        else {
            styleFormats = fallbackStyles;
        }

        return $q.all(promises).then(function() {
            return $q.when({ stylesheets: stylesheets, styleFormats: styleFormats});
        });
    }

    /** Returns the language to use for TinyMCE */
    function getLanguage() {
        var language = defaultLanguage;
        //get locale from angular and match tinymce format. Angular localization is always in the format of ru-ru, de-de, en-gb, etc.
        //wheras tinymce is in the format of ru, de, en, en_us, etc.
        var localeId = $locale.id.replace('-', '_');
        //try matching the language using full locale format
        var languageMatch = _.find(availableLanguages, function (o) { return o === localeId; });
        //if no matches, try matching using only the language
        if (languageMatch === undefined) {
            var localeParts = localeId.split('_');
            languageMatch = _.find(availableLanguages, function (o) { return o === localeParts[0]; });
        }
        //if a match was found - set the language
        if (languageMatch !== undefined) {
            language = languageMatch;
        }
        return language;
    }

    /**
     * Gets toolbars for the inlite theme
     * @param {any} configuredToolbar
     * @param {any} tinyMceConfig
     */
    function getToolbars(configuredToolbar, tinyMceConfig) {

        //the commands for selection/all
        var allowedSelectionToolbar = _.map(_.filter(tinyMceConfig.commands,
                function(f) {
                    return f.mode === "Selection" || f.mode === "All";
                }),
            function(f) {
                return f.alias;
            });

        //the commands for insert/all
        var allowedInsertToolbar = _.map(_.filter(tinyMceConfig.commands,
                function(f) {
                    return f.mode === "Insert" || f.mode === "All";
                }),
            function(f) {
                return f.alias;
            });

        var insertToolbar = _.filter(configuredToolbar, function (t) {
            return allowedInsertToolbar.indexOf(t) !== -1;
        }).join(" | ");

        var selectionToolbar = _.filter(configuredToolbar, function (t) {
            return allowedSelectionToolbar.indexOf(t) !== -1;
        }).join(" | ");

        return {
            insertToolbar: insertToolbar,
            selectionToolbar: selectionToolbar
        }
    }

    return {

        /**
         * Returns a promise of the configuration object to initialize the TinyMCE editor
         * @param {} args
         * @returns {}
         */
        getTinyMceEditorConfig: function (args) {

            var promises = [
                this.configuration(),
                getStyles(args.stylesheets)
            ];

            return $q.all(promises).then(function(result) {

                var tinyMceConfig = result[0];
                var styles = result[1];

                var toolbars = getToolbars(args.toolbar, tinyMceConfig);

                var plugins = _.map(tinyMceConfig.plugins, function (plugin) {
                    return plugin.name;
                });

                //plugins that must always be active
                plugins.push("autoresize");
                plugins.push("noneditable");

                var modeTheme = '';
                var modeInline = false;


                //Based on mode set
                //classic = Theme: modern, inline: false
                //inline = Theme: modern, inline: true,
                //distraction-free = Theme: inlite, inline: true
                switch (args.mode) {
                    case "classic":
                        modeTheme  = "modern";
                        modeInline = false;
                        break;

                    case "distraction-free":
                        modeTheme = "inlite";
                        modeInline = true;
                        break;

                    default:
                        //Will default to 'classic'
                        modeTheme  = "modern";
                        modeInline = false;
                        break;
                }



                //create a baseline Config to exten upon
                var config = {
                    selector: "#" + args.htmlId,
                    theme: modeTheme,
                    //skin: "umbraco",
                    inline: modeInline,
                    plugins: plugins,
                    valid_elements: tinyMceConfig.validElements,
                    invalid_elements: tinyMceConfig.inValidElements,
                    extended_valid_elements: extendedValidElements,
                    menubar: false,
                    statusbar: false,
                    relative_urls: false,
                    autoresize_bottom_margin: 10,
                    content_css: styles.stylesheets,
                    style_formats: styles.styleFormats,
                    language: getLanguage(),

                    //this would be for a theme other than inlite
                    toolbar: args.toolbar.join(" "),
                    //these are for the inlite theme to work
                    insert_toolbar: toolbars.insertToolbar,
                    selection_toolbar: toolbars.selectionToolbar,

                    body_class: 'umb-rte',
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
                                    if (angular.isArray(config[i]) && angular.isArray(tinyMceConfig.customConfig[i])) {
                                        //concat it and below this concat'd array will overwrite the baseline in angular.extend
                                        tinyMceConfig.customConfig[i] = config[i].concat(tinyMceConfig.customConfig[i]);
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

                    angular.extend(config, tinyMceConfig.customConfig);
                }


                return $q.when(config);

            });

        },

		/**
		 * @ngdoc method
		 * @name umbraco.services.tinyMceService#configuration
		 * @methodOf umbraco.services.tinyMceService
		 *
		 * @description
		 * Returns a collection of plugins available to the tinyMCE editor
		 *
		 */
        configuration: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "rteApiBaseUrl",
                        "GetConfiguration"), {
                        cache: true
                    }),
                'Failed to retrieve tinymce configuration');
        },

		/**
		 * @ngdoc method
		 * @name umbraco.services.tinyMceService#defaultPrevalues
		 * @methodOf umbraco.services.tinyMceService
		 *
		 * @description
		 * Returns a default configration to fallback on in case none is provided
		 *
		 */
        defaultPrevalues: function () {
            var cfg = {};
            cfg.toolbar = ["code", "bold", "italic", "styleselect", "alignleft", "aligncenter", "alignright", "bullist", "numlist", "outdent", "indent", "link", "image", "umbmediapicker", "umbembeddialog", "umbmacro"];
            cfg.stylesheets = [];
            cfg.maxImageSize = 500;
            return cfg;
        },

		/**
		 * @ngdoc method
		 * @name umbraco.services.tinyMceService#createInsertEmbeddedMedia
		 * @methodOf umbraco.services.tinyMceService
		 *
		 * @description
		 * Creates the umbrco insert embedded media tinymce plugin
		 *
		 * @param {Object} editor the TinyMCE editor instance
		 */
        createInsertEmbeddedMedia: function (editor, callback) {
            editor.addButton('umbembeddialog', {
                icon: 'custom icon-tv',
                tooltip: 'Embed',
                onclick: function () {
                    if (callback) {
                        angularHelper.safeApply($rootScope, function() {
                            callback();
                        });
                    }
                }
            });
        },

        insertEmbeddedMediaInEditor: function (editor, preview) {
            editor.insertContent(preview);
        },


        createAceCodeEditor: function(editor, callback){

            editor.addButton("ace", {
                icon: "code",
                tooltip: "View Source Code",
                onclick: function(){
                    callback();
                }
            });

        },

		/**
		 * @ngdoc method
		 * @name umbraco.services.tinyMceService#createMediaPicker
		 * @methodOf umbraco.services.tinyMceService
		 *
		 * @description
		 * Creates the umbrco insert media tinymce plugin
		 *
		 * @param {Object} editor the TinyMCE editor instance
		 */
        createMediaPicker: function (editor, callback) {
            editor.addButton('umbmediapicker', {
                icon: 'custom icon-picture',
                tooltip: 'Media Picker',
                stateSelector: 'img',
                onclick: function () {


                    var selectedElm = editor.selection.getNode(),
                        currentTarget;

                    if (selectedElm.nodeName === 'IMG') {
                        var img = $(selectedElm);

                        var hasUdi = img.attr("data-udi") ? true : false;

                        currentTarget = {
                            altText: img.attr("alt"),
                            url: img.attr("src")
                        };

                        if (hasUdi) {
                            currentTarget["udi"] = img.attr("data-udi");
                        } else {
                            currentTarget["id"] = img.attr("rel");
                        }
                    }

                    userService.getCurrentUser().then(function (userData) {
                        if (callback) {
                            angularHelper.safeApply($rootScope, function() {
                                callback(currentTarget, userData);
                            });
                        }
                    });
                }
            });
        },

        insertMediaInEditor: function (editor, img) {
            if (img) {

                var hasUdi = img.udi ? true : false;

                var data = {
                    alt: img.altText || "",
                    src: (img.url) ? img.url : "nothing.jpg",
                    id: '__mcenew'
                };

                if (hasUdi) {
                    data["data-udi"] = img.udi;
                } else {
                    //Considering these fixed because UDI will now be used and thus
                    // we have no need for rel http://issues.umbraco.org/issue/U4-6228, http://issues.umbraco.org/issue/U4-6595
                    data["rel"] = img.id;
                    data["data-id"] = img.id;
                }

                editor.insertContent(editor.dom.createHTML('img', data));

                $timeout(function () {
                    var imgElm = editor.dom.get('__mcenew');
                    var size = editor.dom.getSize(imgElm);

                    if (editor.settings.maxImageSize && editor.settings.maxImageSize !== 0) {
                        var newSize = imageHelper.scaleToMaxSize(editor.settings.maxImageSize, size.w, size.h);

                        var s = "width: " + newSize.width + "px; height:" + newSize.height + "px;";
                        editor.dom.setAttrib(imgElm, 'style', s);

                        if (img.url) {
                            var src = img.url + "?width=" + newSize.width + "&height=" + newSize.height;
                            editor.dom.setAttrib(imgElm, 'data-mce-src', src);
                        }
                    }
				    editor.dom.setAttrib(imgElm, 'id', null);
                }, 500);
            }
        },

		/**
		 * @ngdoc method
		 * @name umbraco.services.tinyMceService#createUmbracoMacro
		 * @methodOf umbraco.services.tinyMceService
		 *
		 * @description
		 * Creates the insert umbrco macro tinymce plugin
		 *
		 * @param {Object} editor the TinyMCE editor instance
		 */
        createInsertMacro: function (editor, callback) {

            var createInsertMacroScope = this;

            /** Adds custom rules for the macro plugin and custom serialization */
            editor.on('preInit', function (args) {
                //this is requires so that we tell the serializer that a 'div' is actually allowed in the root, otherwise the cleanup will strip it out
                editor.serializer.addRules('div');

                /** This checks if the div is a macro container, if so, checks if its wrapped in a p tag and then unwraps it (removes p tag) */
                editor.serializer.addNodeFilter('div', function (nodes, name) {
                    for (var i = 0; i < nodes.length; i++) {
                        if (nodes[i].attr("class") === "umb-macro-holder" && nodes[i].parent && nodes[i].parent.name.toUpperCase() === "P") {
                            nodes[i].parent.unwrap();
                        }
                    }
                });

            });

			/**
			 * Because the macro gets wrapped in a P tag because of the way 'enter' works, this
			 * method will return the macro element if not wrapped in a p, or the p if the macro
			 * element is the only one inside of it even if we are deep inside an element inside the macro
			 */
            function getRealMacroElem(element) {
                var e = $(element).closest(".umb-macro-holder");
                if (e.length > 0) {
                    if (e.get(0).parentNode.nodeName === "P") {
                        //now check if we're the only element
                        if (element.parentNode.childNodes.length === 1) {
                            return e.get(0).parentNode;
                        }
                    }
                    return e.get(0);
                }
                return null;
            }

            /** Adds the button instance */
            editor.addButton('umbmacro', {
                icon: 'custom icon-settings-alt',
                tooltip: 'Insert macro',
                onPostRender: function () {

                    var ctrl = this;
                    var isOnMacroElement = false;

					/**
					 if the selection comes from a different element that is not the macro's
					 we need to check if the selection includes part of the macro, if so we'll force the selection
					 to clear to the next element since if people can select part of the macro markup they can then modify it.
					*/
                    function handleSelectionChange() {

                        if (!editor.selection.isCollapsed()) {
                            var endSelection = tinymce.activeEditor.selection.getEnd();
                            var startSelection = tinymce.activeEditor.selection.getStart();
                            //don't proceed if it's an entire element selected
                            if (endSelection !== startSelection) {

                                //if the end selection is a macro then move the cursor
                                //NOTE: we don't have to handle when the selection comes from a previous parent because
                                // that is automatically taken care of with the normal onNodeChanged logic since the
                                // evt.element will be the macro once it becomes part of the selection.
                                var $testForMacro = $(endSelection).closest(".umb-macro-holder");
                                if ($testForMacro.length > 0) {

                                    //it came from before so move after, if there is no after then select ourselves
                                    var next = $testForMacro.next();
                                    if (next.length > 0) {
                                        editor.selection.setCursorLocation($testForMacro.next().get(0));
                                    } else {
                                        selectMacroElement($testForMacro.get(0));
                                    }

                                }
                            }
                        }
                    }

                    /** helper method to select the macro element */
                    function selectMacroElement(macroElement) {

                        // move selection to top element to ensure we can't edit this
                        editor.selection.select(macroElement);

                        // check if the current selection *is* the element (ie bug)
                        var currentSelection = editor.selection.getStart();
                        if (tinymce.isIE) {
                            if (!editor.dom.hasClass(currentSelection, 'umb-macro-holder')) {
                                while (!editor.dom.hasClass(currentSelection, 'umb-macro-holder') && currentSelection.parentNode) {
                                    currentSelection = currentSelection.parentNode;
                                }
                                editor.selection.select(currentSelection);
                            }
                        }
                    }

					/**
					 * Add a node change handler, test if we're editing a macro and select the whole thing, then set our isOnMacroElement flag.
					 * If we change the selection inside this method, then we end up in an infinite loop, so we have to remove ourselves
					 * from the event listener before changing selection, however, it seems that putting a break point in this method
					 * will always cause an 'infinite' loop as the caret keeps changing.
					 *
					 * TODO: I don't think we need this anymore with recent tinymce fixes: https://www.tiny.cloud/docs/plugins/noneditable/
					 */
                    function onNodeChanged(evt) {

                        //set our macro button active when on a node of class umb-macro-holder
                        var $macroElement = $(evt.element).closest(".umb-macro-holder");

                        handleSelectionChange();

                        //set the button active
                        ctrl.active($macroElement.length !== 0);

                        if ($macroElement.length > 0) {
                            var macroElement = $macroElement.get(0);

                            //remove the event listener before re-selecting
                            editor.off('NodeChange', onNodeChanged);

                            selectMacroElement(macroElement);

                            //set the flag
                            isOnMacroElement = true;

                            //re-add the event listener
                            editor.on('NodeChange', onNodeChanged);
                        } else {
                            isOnMacroElement = false;
                        }

                    }

                    /** when the contents load we need to find any macros declared and load in their content */
                    editor.on("LoadContent", function (o) {

                        //get all macro divs and load their content
                        $(editor.dom.select(".umb-macro-holder.mceNonEditable")).each(function () {
                            createInsertMacroScope.loadMacroContent($(this), null);
                        });

                    });

                    /**
                     * This prevents any other commands from executing when the current element is the macro so the content cannot be edited
                     *
                     * TODO: I don't think we need this anymore with recent tinymce fixes: https://www.tiny.cloud/docs/plugins/noneditable/
                     */
                    editor.on('BeforeExecCommand', function (o) {
                        if (isOnMacroElement) {
                            if (o.preventDefault) {
                                o.preventDefault();
                            }
                            if (o.stopImmediatePropagation) {
                                o.stopImmediatePropagation();
                            }
                            return;
                        }
                    });

                    /**
                     * This double checks and ensures you can't paste content into the rendered macro
                     *
                     * TODO: I don't think we need this anymore with recent tinymce fixes: https://www.tiny.cloud/docs/plugins/noneditable/
                     */
                    editor.on("Paste", function (o) {
                        if (isOnMacroElement) {
                            if (o.preventDefault) {
                                o.preventDefault();
                            }
                            if (o.stopImmediatePropagation) {
                                o.stopImmediatePropagation();
                            }
                            return;
                        }
                    });

                    //set onNodeChanged event listener
                    editor.on('NodeChange', onNodeChanged);

					/**
					 * Listen for the keydown in the editor, we'll check if we are currently on a macro element, if so
					 * we'll check if the key down is a supported key which requires an action, otherwise we ignore the request
					 * so the macro cannot be edited.
					 *
					 * TODO: I don't think we need this anymore with recent tinymce fixes: https://www.tiny.cloud/docs/plugins/noneditable/
					 */
                    editor.on('KeyDown', function (e) {
                        if (isOnMacroElement) {
                            var macroElement = editor.selection.getNode();

                            //get the 'real' element (either p or the real one)
                            macroElement = getRealMacroElem(macroElement);

                            //prevent editing
                            e.preventDefault();
                            e.stopPropagation();

                            var moveSibling = function (element, isNext) {
                                var $e = $(element);
                                var $sibling = isNext ? $e.next() : $e.prev();
                                if ($sibling.length > 0) {
                                    editor.selection.select($sibling.get(0));
                                    editor.selection.collapse(true);
                                } else {
                                    //if we're moving previous and there is no sibling, then lets recurse and just select the next one
                                    if (!isNext) {
                                        moveSibling(element, true);
                                        return;
                                    }

                                    //if there is no sibling we'll generate a new p at the end and select it
                                    editor.setContent(editor.getContent() + "<p>&nbsp;</p>");
                                    editor.selection.select($(editor.dom.getRoot()).children().last().get(0));
                                    editor.selection.collapse(true);

                                }
                            };

                            //supported keys to move to the next or prev element (13-enter, 27-esc, 38-up, 40-down, 39-right, 37-left)
                            //supported keys to remove the macro (8-backspace, 46-delete)
                            //TODO: Should we make the enter key insert a line break before or leave it as moving to the next element?
                            if ($.inArray(e.keyCode, [13, 40, 39]) !== -1) {
                                //move to next element
                                moveSibling(macroElement, true);
                            } else if ($.inArray(e.keyCode, [27, 38, 37]) !== -1) {
                                //move to prev element
                                moveSibling(macroElement, false);
                            } else if ($.inArray(e.keyCode, [8, 46]) !== -1) {
                                //delete macro element

                                //move first, then delete
                                moveSibling(macroElement, false);
                                editor.dom.remove(macroElement);
                            }
                            return;
                        }
                    });

                },

                /** The insert macro button click event handler */
                onclick: function () {

                    var dialogData = {
                        //flag for use in rte so we only show macros flagged for the editor
                        richTextEditor: true
                    };

                    //when we click we could have a macro already selected and in that case we'll want to edit the current parameters
                    //so we'll need to extract them and submit them to the dialog.
                    var macroElement = editor.selection.getNode();
                    macroElement = getRealMacroElem(macroElement);
                    if (macroElement) {
                        //we have a macro selected so we'll need to parse it's alias and parameters
                        var contents = $(macroElement).contents();
                        var comment = _.find(contents, function (item) {
                            return item.nodeType === 8;
                        });
                        if (!comment) {
                            throw "Cannot parse the current macro, the syntax in the editor is invalid";
                        }
                        var syntax = comment.textContent.trim();
                        var parsed = macroService.parseMacroSyntax(syntax);
                        dialogData = {
                            macroData: parsed
                        };
                    }

                    if (callback) {
                        angularHelper.safeApply($rootScope, function () {
                            callback(dialogData);
                        });
                    }
                }
            });
        },

        insertMacroInEditor: function (editor, macroObject) {

            //put the macro syntax in comments, we will parse this out on the server side to be used
            //for persisting.
            var macroSyntaxComment = "<!-- " + macroObject.syntax + " -->";
            //create an id class for this element so we can re-select it after inserting
            var uniqueId = "umb-macro-" + editor.dom.uniqueId();
            var macroDiv = editor.dom.create('div',
                {
                    'class': 'umb-macro-holder ' + macroObject.macroAlias + ' mceNonEditable ' + uniqueId
                },
                macroSyntaxComment + '<ins>Macro alias: <strong>' + macroObject.macroAlias + '</strong></ins>');

            editor.selection.setNode(macroDiv);

            var $macroDiv = $(editor.dom.select("div.umb-macro-holder." + uniqueId));

            //async load the macro content
            this.loadMacroContent($macroDiv, macroObject);

        },

        /** loads in the macro content async from the server */
        loadMacroContent: function ($macroDiv, macroData) {

            //if we don't have the macroData, then we'll need to parse it from the macro div
            if (!macroData) {
                var contents = $macroDiv.contents();
                var comment = _.find(contents, function (item) {
                    return item.nodeType === 8;
                });
                if (!comment) {
                    throw "Cannot parse the current macro, the syntax in the editor is invalid";
                }
                var syntax = comment.textContent.trim();
                var parsed = macroService.parseMacroSyntax(syntax);
                macroData = parsed;
            }

            var $ins = $macroDiv.find("ins");

            //show the throbber
            $macroDiv.addClass("loading");

            var contentId = $routeParams.id;

            //need to wrap in safe apply since this might be occuring outside of angular
            angularHelper.safeApply($rootScope, function () {
                macroResource.getMacroResultAsHtmlForEditor(macroData.macroAlias, contentId, macroData.macroParamsDictionary)
                    .then(function (htmlResult) {

                        $macroDiv.removeClass("loading");
                        htmlResult = htmlResult.trim();
                        if (htmlResult !== "") {
                            $ins.html(htmlResult);
                        }
                    });
            });

        },

        createLinkPicker: function (editor, onClick) {

            function createLinkList(callback) {
                return function () {
                    var linkList = editor.settings.link_list;

                    if (typeof (linkList) === "string") {
                        tinymce.util.XHR.send({
                            url: linkList,
                            success: function (text) {
                                callback(tinymce.util.JSON.parse(text));
                            }
                        });
                    } else {
                        callback(linkList);
                    }
                };
            }

            function showDialog(linkList) {
                var data = {},
                    selection = editor.selection,
                    dom = editor.dom,
                    selectedElm, anchorElm, initialText;
                var win, linkListCtrl, relListCtrl, targetListCtrl;

                function linkListChangeHandler(e) {
                    var textCtrl = win.find('#text');

                    if (!textCtrl.value() || (e.lastControl && textCtrl.value() === e.lastControl.text())) {
                        textCtrl.value(e.control.text());
                    }

                    win.find('#href').value(e.control.value());
                }

                function buildLinkList() {
                    var linkListItems = [{
                        text: 'None',
                        value: ''
                    }];

                    tinymce.each(linkList, function (link) {
                        linkListItems.push({
                            text: link.text || link.title,
                            value: link.value || link.url,
                            menu: link.menu
                        });
                    });

                    return linkListItems;
                }

                function buildRelList(relValue) {
                    var relListItems = [{
                        text: 'None',
                        value: ''
                    }];

                    tinymce.each(editor.settings.rel_list, function (rel) {
                        relListItems.push({
                            text: rel.text || rel.title,
                            value: rel.value,
                            selected: relValue === rel.value
                        });
                    });

                    return relListItems;
                }

                function buildTargetList(targetValue) {
                    var targetListItems = [{
                        text: 'None',
                        value: ''
                    }];

                    if (!editor.settings.target_list) {
                        targetListItems.push({
                            text: 'New window',
                            value: '_blank'
                        });
                    }

                    tinymce.each(editor.settings.target_list, function (target) {
                        targetListItems.push({
                            text: target.text || target.title,
                            value: target.value,
                            selected: targetValue === target.value
                        });
                    });

                    return targetListItems;
                }

                function buildAnchorListControl(url) {
                    var anchorList = [];

                    tinymce.each(editor.dom.select('a:not([href])'), function (anchor) {
                        var id = anchor.name || anchor.id;

                        if (id) {
                            anchorList.push({
                                text: id,
                                value: '#' + id,
                                selected: url.indexOf('#' + id) !== -1
                            });
                        }
                    });

                    if (anchorList.length) {
                        anchorList.unshift({
                            text: 'None',
                            value: ''
                        });

                        return {
                            name: 'anchor',
                            type: 'listbox',
                            label: 'Anchors',
                            values: anchorList,
                            onselect: linkListChangeHandler
                        };
                    }
                }

                function updateText() {
                    if (!initialText && data.text.length === 0) {
                        this.parent().parent().find('#text')[0].value(this.value());
                    }
                }

                selectedElm = selection.getNode();
                anchorElm = dom.getParent(selectedElm, 'a[href]');

                data.text = initialText = anchorElm ? (anchorElm.innerText || anchorElm.textContent) : selection.getContent({
                    format: 'text'
                });
                data.href = anchorElm ? dom.getAttrib(anchorElm, 'href') : '';
                data.target = anchorElm ? dom.getAttrib(anchorElm, 'target') : '';
                data.rel = anchorElm ? dom.getAttrib(anchorElm, 'rel') : '';

                if (selectedElm.nodeName === "IMG") {
                    data.text = initialText = " ";
                }

                if (linkList) {
                    linkListCtrl = {
                        type: 'listbox',
                        label: 'Link list',
                        values: buildLinkList(),
                        onselect: linkListChangeHandler
                    };
                }

                if (editor.settings.target_list !== false) {
                    targetListCtrl = {
                        name: 'target',
                        type: 'listbox',
                        label: 'Target',
                        values: buildTargetList(data.target)
                    };
                }

                if (editor.settings.rel_list) {
                    relListCtrl = {
                        name: 'rel',
                        type: 'listbox',
                        label: 'Rel',
                        values: buildRelList(data.rel)
                    };
                }

                var currentTarget = null;

                //if we already have a link selected, we want to pass that data over to the dialog
                if (anchorElm) {
                    var anchor = $(anchorElm);
                    currentTarget = {
                        name: anchor.attr("title"),
                        url: anchor.attr("href"),
                        target: anchor.attr("target")
                    };

                    // drop the lead char from the anchor text, if it has a value
                    var anchorVal = anchor[0].dataset.anchor;
                    if (anchorVal) {
                        currentTarget.anchor = anchorVal.substring(1);
                    }

		    //locallink detection, we do this here, to avoid poluting the editorService
		    //so the editor service can just expect to get a node-like structure
                    if (currentTarget.url.indexOf("localLink:") > 0) {
                        // if the current link has an anchor, it needs to be considered when getting the udi/id
                        // if an anchor exists, reduce the substring max by its length plus two to offset the removed prefix and trailing curly brace
                        var linkId = currentTarget.url.substring(currentTarget.url.indexOf(":") + 1, currentTarget.url.lastIndexOf("}"));

                        //we need to check if this is an INT or a UDI
                        var parsedIntId = parseInt(linkId, 10);
                        if (isNaN(parsedIntId)) {
                            //it's a UDI
                            currentTarget.udi = linkId;
                        } else {
                            currentTarget.id = linkId;
                        }
                    }
                }

                angularHelper.safeApply($rootScope,
                    function () {
                        if (onClick) {
                            onClick(currentTarget, anchorElm);
                        }
                    });
            }

            editor.addButton('link', {
                icon: 'link',
                tooltip: 'Insert/edit link',
                shortcut: 'Ctrl+K',
                onclick: createLinkList(showDialog),
                stateSelector: 'a[href]'
            });

            editor.addButton('unlink', {
                icon: 'unlink',
                tooltip: 'Remove link',
                cmd: 'unlink',
                stateSelector: 'a[href]'
            });

            editor.addShortcut('Ctrl+K', '', createLinkList(showDialog));
            this.showDialog = showDialog;

            editor.addMenuItem('link', {
                icon: 'link',
                text: 'Insert link',
                shortcut: 'Ctrl+K',
                onclick: createLinkList(showDialog),
                stateSelector: 'a[href]',
                context: 'insert',
                prependToContext: true
            });

        },

		/**
		 * @ngdoc method
		 * @name umbraco.services.tinyMceService#getAnchorNames
		 * @methodOf umbraco.services.tinyMceService
		 *
		 * @description
		 * From the given string, generates a string array where each item is the id attribute value from a named anchor
		 * 'some string <a id="anchor"></a>with a named anchor' returns ['anchor']
		 *
		 * @param {string} input the string to parse
		 */
        getAnchorNames: function (input) {
        var anchors = [];
        if (!input) {
            return anchors;
        }

            var anchorPattern = /<a id=\\"(.*?)\\">/gi;
            var matches = input.match(anchorPattern);


            if (matches) {
                anchors = matches.map(function (v) {
                    return v.substring(v.indexOf('"') + 1, v.lastIndexOf('\\'));
                });
            }

	    return anchors.filter(function(val, i, self) {
              return self.indexOf(val) === i;
            });
        },

        insertLinkInEditor: function (editor, target, anchorElm) {

            var href = target.url;
            // We want to use the Udi. If it is set, we use it, else fallback to id, and finally to null
            var hasUdi = target.udi ? true : false;
            var id = hasUdi ? target.udi : (target.id ? target.id : null);

            // if an anchor exists, check that it is appropriately prefixed
            if (target.anchor && target.anchor[0] !== '?' && target.anchor[0] !== '#') {
                target.anchor = (target.anchor.indexOf('=') === -1 ? '#' : '?') + target.anchor;
            }

            // the href might be an external url, so check the value for an anchor/qs
            // href has the anchor re-appended later, hence the reset here to avoid duplicating the anchor
            if (!target.anchor) {
                var urlParts = href.split(/(#|\?)/);
                if (urlParts.length === 3) {
                    href = urlParts[0];
                    target.anchor = urlParts[1] + urlParts[2];
                }
            }

            //Create a json obj used to create the attributes for the tag
            function createElemAttributes() {
                var a = {
                    href: href,
                    title: target.name,
                    target: target.target ? target.target : null,
                    rel: target.rel ? target.rel : null
                };

                if (hasUdi) {
                    a["data-udi"] = target.udi;
                } else if (target.id) {
                    a["data-id"] = target.id;
                }

                if (target.anchor) {
                    a["data-anchor"] = target.anchor;
                    a.href = a.href + target.anchor;
                } else {
                    a["data-anchor"] = null;
                }

                return a;
            }

            function insertLink() {
                if (anchorElm) {
                    editor.dom.setAttribs(anchorElm, createElemAttributes());

                    editor.selection.select(anchorElm);
                    editor.execCommand('mceEndTyping');
                } else {
                    editor.execCommand('mceInsertLink', false, createElemAttributes());
                }
            }

            if (!href) {
                editor.execCommand('unlink');
                return;
            }

            //if we have an id, it must be a locallink:id, aslong as the isMedia flag is not set
            if (id && (angular.isUndefined(target.isMedia) || !target.isMedia)) {

                href = "/{localLink:" + id + "}";

                insertLink();
                return;
            }

            // Is email and not //user@domain.com
            if (href.indexOf('@') > 0 && href.indexOf('//') === -1 && href.indexOf('mailto:') === -1) {
                href = 'mailto:' + href;
                insertLink();
                return;
            }

            // Is www. prefixed
            if (/^\s*www\./i.test(href)) {
                href = 'http://' + href;
                insertLink();
                return;
            }

            insertLink();

        },

        pinToolbar : function (editor) {

            var tinyMce = $(editor.editorContainer);
            var toolbar = tinyMce.find(".mce-toolbar");
            var toolbarHeight = toolbar.height();
            var tinyMceRect = tinyMce[0].getBoundingClientRect();
            var tinyMceTop = tinyMceRect.top;
            var tinyMceBottom = tinyMceRect.bottom;
            var tinyMceWidth = tinyMceRect.width;

            var tinyMceEditArea = tinyMce.find(".mce-edit-area");

            // set padding in top of mce so the content does not "jump" up
            tinyMceEditArea.css("padding-top", toolbarHeight);

            if (tinyMceTop < 177 && ((177 + toolbarHeight) < tinyMceBottom)) {
                toolbar
                    .css("visibility", "visible")
                    .css("position", "fixed")
                    .css("top", "177px")
                    .css("margin-top", "0")
                    .css("width", tinyMceWidth);
            } else {
                toolbar
                    .css("visibility", "visible")
                    .css("position", "absolute")
                    .css("top", "auto")
                    .css("margin-top", "0")
                    .css("width", tinyMceWidth);
            }

        },

        unpinToolbar: function (editor) {

            var tinyMce = $(editor.editorContainer);
            var toolbar = tinyMce.find(".mce-toolbar");
            var tinyMceEditArea = tinyMce.find(".mce-edit-area");
            // reset padding in top of mce so the content does not "jump" up
            tinyMceEditArea.css("padding-top", "0");
            toolbar.css("position", "static");
        },

        /** Helper method to initialize the tinymce editor within Umbraco */
        initializeEditor: function (args) {

            if (!args.editor) {
                throw "args.editor is required";
            }
            //if (!args.model.value) {
            //    throw "args.model.value is required";
            //}

            var unwatch = null;

            //Starts a watch on the model value so that we can update TinyMCE if the model changes behind the scenes or from the server
            function startWatch() {
                unwatch = $rootScope.$watch(() => args.model.value, function (newVal, oldVal) {
                    if (newVal !== oldVal) {
                        //update the display val again if it has changed from the server;
                        //uses an empty string in the editor when the value is null
                        args.editor.setContent(newVal || "", { format: 'raw' });

                        //we need to manually fire this event since it is only ever fired based on loading from the DOM, this
                        // is required for our plugins listening to this event to execute
                        args.editor.fire('LoadContent', null);
                    }
                });
            }

            //Stops the watch on model.value which is done anytime we are manually updating the model.value
            function stopWatch() {
                if (unwatch) {
                    unwatch();
                }
            }

            function syncContent() {

                //stop watching before we update the value
                stopWatch();
                angularHelper.safeApply($rootScope, function () {
                    args.model.value = args.editor.getContent();
                });
                //re-watch the value
                startWatch();
            }

            args.editor.on('init', function (e) {

                if (args.model.value) {
                    args.editor.setContent(args.model.value);
                }
                //enable browser based spell checking
                args.editor.getBody().setAttribute('spellcheck', true);
            });

            args.editor.on('Change', function (e) {
                syncContent();
            });

            //when we leave the editor (maybe)
            args.editor.on('blur', function (e) {
                syncContent();
            });

            args.editor.on('ObjectResized', function (e) {
                var qs = "?width=" + e.width + "&height=" + e.height + "&mode=max";
                var srcAttr = $(e.target).attr("src");
                var path = srcAttr.split("?")[0];
                $(e.target).attr("data-mce-src", path + qs);

                syncContent();
            });

            args.editor.on('Dirty', function (e) {
                //make the form dirty manually so that the track changes works, setting our model doesn't trigger
                // the angular bits because tinymce replaces the textarea.
                if (args.currentForm) {
                    args.currentForm.$setDirty();
                }
            });

            var self = this;

            //create link picker
            self.createLinkPicker(args.editor, function (currentTarget, anchorElement) {
                var linkPicker = {
                    currentTarget: currentTarget,
                    anchors: self.getAnchorNames(JSON.stringify(editorState.current.properties)),
                    submit: function (model) {
                        self.insertLinkInEditor(args.editor, model.target, anchorElement);
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.linkPicker(linkPicker);
            });

            //Create the insert media plugin
            self.createMediaPicker(args.editor, function (currentTarget, userData) {
                var mediaPicker = {
                    currentTarget: currentTarget,
                    onlyImages: true,
                    showDetails: true,
                    disableFolderSelect: true,
                    startNodeId: userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0],
                    startNodeIsVirtual: userData.startMediaIds.length !== 1,
                    submit: function (model) {
                        self.insertMediaInEditor(args.editor, model.selectedImages[0]);
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.mediaPicker(mediaPicker);
            });

            //Create the embedded plugin
            self.createInsertEmbeddedMedia(args.editor, function () {
                var embed = {
                    submit: function (model) {
                        self.insertEmbeddedMediaInEditor(args.editor, model.embed.preview);
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.embed(embed);
            });

            //Create the insert macro plugin
            self.createInsertMacro(args.editor, function (dialogData) {
                var macroPicker = {
                    dialogData: dialogData,
                    submit: function (model) {
                        var macroObject = macroService.collectValueData(model.selectedMacro, model.macroParams, dialogData.renderingEngine);
                        self.insertMacroInEditor(args.editor, macroObject, $scope);
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.macroPicker(macroPicker);
            });

            self.createAceCodeEditor(args.editor, function () {

                //TODO: CHECK TO SEE WHAT WE NEED TO DO WIT MACROS (See code block?)
                /*
                var html = editor.getContent({source_view: true});
                html = html.replace(/<span\s+class="CmCaReT"([^>]*)>([^<]*)<\/span>/gm, String.fromCharCode(chr));
                editor.dom.remove(editor.dom.select('.CmCaReT'));
                html = html.replace(/(<div class=".*?umb-macro-holder.*?mceNonEditable.*?"><!-- <\?UMBRACO_MACRO macroAlias="(.*?)".*?\/> --> *<ins>)[\s\S]*?(<\/ins> *<\/div>)/ig, "$1Macro alias: <strong>$2</strong>$3");
                */

                var aceEditor = {
                    content: args.editor.getContent(),
                    view: 'views/propertyeditors/rte/codeeditor.html',
                    size: 'small',
                    submit: function (model) {
                        args.editor.setContent(model.content);
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };

                editorService.open(aceEditor);
            });

            //start watching the value
            startWatch(args.editor);
        }

    };
}

angular.module('umbraco.services').factory('tinyMceService', tinyMceService);
