/**
 * @ngdoc service
 * @name umbraco.services.tinyMceService
 *
 *
 * @description
 * A service containing all logic for all of the Umbraco TinyMCE plugins
 */
function tinyMceService($rootScope, $q, imageHelper, $locale, $http, $timeout, stylesheetResource, macroResource, macroService,
                        $routeParams, umbRequestHelper, angularHelper, userService, editorService, entityResource, eventsService, localStorageService, mediaHelper) {

    //These are absolutely required in order for the macros to render inline
    //we put these as extended elements because they get merged on top of the normal allowed elements by tiny mce
  var extendedValidElements = "@[id|class|style],-div[id|dir|class|align|style],ins[datetime|cite],-ul[class|style],-li[class|style],-h1[id|dir|class|align|style],-h2[id|dir|class|align|style],-h3[id|dir|class|align|style],-h4[id|dir|class|align|style],-h5[id|dir|class|align|style],-h6[id|style|dir|class|align],span[id|class|style|lang],figure,figcaption";
    var fallbackStyles = [{ title: "Page header", block: "h2" }, { title: "Section header", block: "h3" }, { title: "Paragraph header", block: "h4" }, { title: "Normal", block: "p" }, { title: "Quote", block: "blockquote" }, { title: "Code", block: "code" }];
    // these languages are available for localization
    var availableLanguages = [
        'ar',
        'ar_SA',
        'hy',
        'az',
        'eu',
        'be',
        'bn_BD',
        'bs',
        'bg_BG',
        'ca',
        'zh_CN',
        'zh_TW',
        'hr',
        'cs',
        'da',
        'dv',
        'nl',
        'en_CA',
        'en_GB',
        'et',
        'fo',
        'fi',
        'fr_FR',
        'gd',
        'gl',
        'ka_GE',
        'de',
        'de_AT',
        'el',
        'he_IL',
        'hi_IN',
        'hu_HU',
        'is_IS',
        'id',
        'it',
        'ja',
        'kab',
        'kk',
        'km_KH',
        'ko_KR',
        'ku',
        'ku_IQ',
        'lv',
        'lt',
        'lb',
        'ml',
        'ml_IN',
        'mn_MN',
        'nb_NO',
        'fa',
        'fa_IR',
        'pl',
        'pt_BR',
        'pt_PT',
        'ro',
        'ru',
        'sr',
        'si_LK',
        'sk',
        'sl_SI',
        'es',
        'es_MX',
        'sv_SE',
        'tg',
        'ta',
        'ta_IN',
        'tt',
        'th_TH',
        'tr',
        'tr_TR',
        'ug',
        'uk',
        'uk_UA',
        'vi',
        'vi_VN',
        'cy'
    ];
    //define fallback language
    var defaultLanguage = 'en_US';

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
            configuredStylesheets.forEach(function (val, key) {

                if (val.indexOf(Umbraco.Sys.ServerVariables.umbracoSettings.cssPath + "/") === 0) {
                    // current format (full path to stylesheet)
                    stylesheets.push(val);
                }
                else {
                    // legacy format (stylesheet name only) - must prefix with stylesheet folder and postfix with ".css"
                    stylesheets.push(Umbraco.Sys.ServerVariables.umbracoSettings.cssPath + "/" + val + ".css");
                }

                promises.push(stylesheetResource.getRulesByName(val).then(function (rules) {
                    rules.forEach(function (rule) {
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
                            r.classes = rule.selector.substring(rule.selector.indexOf(".") + 1).replace(/\./g, " ");
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
            // Always push our Umbraco RTE stylesheet
            // So we can style macros, embed items etc...
            stylesheets.push(`${Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath}/assets/css/rte-content.css`);

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
        var languageMatch = _.find(availableLanguages, function (o) { return o.toLowerCase() === localeId; });
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

    function uploadImageHandler(blobInfo, success, failure, progress){
        const xhr = new XMLHttpRequest();
        xhr.open('POST', Umbraco.Sys.ServerVariables.umbracoUrls.tinyMceApiBaseUrl + 'UploadImage');

        xhr.onloadstart = function(e) {
            angularHelper.safeApply($rootScope, function() {
                eventsService.emit("rte.file.uploading");
            });
        };

        xhr.onloadend = function(e) {
            angularHelper.safeApply($rootScope, function() {
                eventsService.emit("rte.file.uploaded");
            });
        };

        xhr.upload.onprogress = function (e) {
            progress(e.loaded / e.total * 100);
        };

        xhr.onerror = function () {
            failure('Image upload failed due to a XHR Transport error. Code: ' + xhr.status);
        };

        xhr.onload = function () {
            if (xhr.status < 200 || xhr.status >= 300) {
                failure('HTTP Error: ' + xhr.status);
                return;
            }

            let data = xhr.responseText;

            // The response is fitted as an AngularJS resource response and needs to be cleaned of the AngularJS metadata
            data = data.split("\n");

            if (!data.length > 1) {
              failure('Unrecognized text string: ' + data);
              return;
            }

            let json = {};

            try {
              json = JSON.parse(data[1]);
            } catch (e) {
              failure('Invalid JSON: ' + data + ' - ' + e.message);
              return;
            }

            if (!json || typeof json.tmpLocation !== 'string') {
              failure('Invalid JSON: ' + data);
              return;
            }

            // Put temp location into localstorage (used to update the img with data-tmpimg later on)
            localStorageService.set(`tinymce__${blobInfo.blobUri()}`, json.tmpLocation);

            // We set the img src url to be the same as we started
            // The Blob URI is stored in TinyMce's cache
            // so the img still shows in the editor
            success(blobInfo.blobUri());
        };

        const formData = new FormData();
        formData.append('file', blobInfo.blob(), blobInfo.blob().name);

        xhr.send(formData);
    }

    function cleanupPasteData(plugin, args) {

        // Remove spans
        args.content = args.content.replace(/<\s*span[^>]*>(.*?)<\s*\/\s*span>/g, "$1");

        // Convert b to strong.
        args.content = args.content.replace(/<\s*b([^>]*)>(.*?)<\s*\/\s*b([^>]*)>/g, "<strong$1>$2</strong$3>");

        // convert i to em
        args.content = args.content.replace(/<\s*i([^>]*)>(.*?)<\s*\/\s*i([^>]*)>/g, "<em$1>$2</em$3>");


    }

    function sizeImageInEditor(editor, imageDomElement, imgUrl) {
        var size = editor.dom.getSize(imageDomElement);

        if (editor.settings.maxImageSize && editor.settings.maxImageSize !== 0) {
            var newSize = imageHelper.scaleToMaxSize(editor.settings.maxImageSize, size.w, size.h);

            editor.dom.setAttrib(imageDomElement, 'width', newSize.width);
            editor.dom.setAttrib(imageDomElement, 'height', newSize.height);

            // Images inserted via Media Picker will have a URL we can use for ImageResizer QueryStrings
            // Images pasted/dragged in are not persisted to media until saved & thus will need to be added
            if (imgUrl) {
                mediaHelper.getProcessedImageUrl(imgUrl,
                    {
                        width: newSize.width,
                        height: newSize.height
                    })
                    .then(function (resizedImgUrl) {
                        editor.dom.setAttrib(imageDomElement, 'data-mce-src', resizedImgUrl);
                    });
            }

            editor.execCommand("mceAutoResize", false, null, null);
        }
    }

    function isMediaPickerEnabled(toolbarItemArray){
        var insertMediaButtonFound = false;
        toolbarItemArray.forEach(toolbarItem => {
            if(toolbarItem.indexOf("umbmediapicker") > -1){
                insertMediaButtonFound = true;
            }
        });
        return insertMediaButtonFound;
    }

    return {

        /**
         * Returns a promise of the configuration object to initialize the TinyMCE editor
         * @param {} args
         * @returns {}
         */
        getTinyMceEditorConfig: function (args) {

            //global defaults, called before/during init
            tinymce.DOM.events.domLoaded = true;
            tinymce.baseURL = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + "/lib/tinymce/"; // trailing slash important

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

                // Plugins that must always be active
                plugins.push("autoresize");
                plugins.push("noneditable");

                // Table plugin use color picker plugin in table properties
                if (plugins.includes("table")) {
                    plugins.push("colorpicker");
                }

                var modeTheme = '';
                var modeInline = false;

                // Based on mode set
                // classic = Theme: modern, inline: false
                // inline = Theme: modern, inline: true,
                // distraction-free = Theme: inlite, inline: true
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
                    theme: modeTheme,
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

                    body_class: "umb-rte",

                    //see http://archive.tinymce.com/wiki.php/Configuration:cache_suffix
                    cache_suffix: "?umb__rnd=" + Umbraco.Sys.ServerVariables.application.cacheBuster
                };

                // Need to check if we are allowed to UPLOAD images
                // This is done by checking if the insert image toolbar button is available
                if(isMediaPickerEnabled(args.toolbar)){
                    // Update the TinyMCE Config object to allow pasting
                    config.images_upload_handler = uploadImageHandler;
                    config.automatic_uploads = false;
                    config.images_replace_blob_uris = false;

                    // This allows images to be pasted in & stored as Base64 until they get uploaded to server
                    config.paste_data_images = true;
                }


                if (args.htmlId) {
                    config.selector = `[id="${args.htmlId}"]`;
                } else if (args.target) {
                    config.target = args.target;
                }

                /*
                // We are not ready to limit the pasted elements further than default, we will return to this feature. ( TODO: Make this feature an option. )
                // We keep spans here, cause removing spans here also removes b-tags inside of them, instead we strip them out later. (TODO: move this definition to the config file... )
                var validPasteElements = "-strong/b,-em/i,-u,-span,-p,-ol,-ul,-li,-p/div,-a[href|name],sub,sup,strike,br,del,table[width],tr,td[colspan|rowspan|width],th[colspan|rowspan|width],thead,tfoot,tbody,img[src|alt|width|height],ul,ol,li,hr,pre,dl,dt,figure,figcaption,wbr"

                // add elements from user configurated styleFormats to our list of validPasteElements.
                // (This means that we only allow H3-element if its configured as a styleFormat on this specific propertyEditor.)
                var style, i = 0;
                for(; i < styles.styleFormats.length; i++) {
                    style = styles.styleFormats[i];
                    if(style.block) {
                        validPasteElements += "," + style.block;
                    }
                }
                */

                /**
                 The default paste config can be overwritten by defining these properties in the customConfig.
                 */
                var pasteConfig = {

                    paste_remove_styles: true,
                    paste_text_linebreaktype: true, //Converts plaintext linebreaks to br or p elements.
                    paste_strip_class_attributes: "none",

                    //paste_word_valid_elements: validPasteElements,

                    paste_preprocess: cleanupPasteData

                };

                Utilities.extend(config, pasteConfig);

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
                                    if (Utilities.isArray(config[i]) && Utilities.isArray(tinyMceConfig.customConfig[i])) {
                                        //concat it and below this concat'd array will overwrite the baseline in Utilities.extend
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

                    Utilities.extend(config, tinyMceConfig.customConfig);
                }

                return config;

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
            cfg.toolbar = ["ace", "styleselect", "bold", "italic", "alignleft", "aligncenter", "alignright", "bullist", "numlist", "outdent", "indent", "link", "umbmediapicker", "umbmacro", "umbembeddialog"];
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
         * Creates the umbraco insert embedded media tinymce plugin
         *
         * @param {Object} editor the TinyMCE editor instance
         */
        createInsertEmbeddedMedia: function (editor, callback) {
            editor.addButton('umbembeddialog', {
                icon: 'custom icon-tv',
                tooltip: 'Embed',
                stateSelector: 'div[data-embed-url]',
                onclick: function () {

                    // Get the selected element
                    // Check nodename is a DIV and the claslist contains 'embeditem'
                    var selectedElm = editor.selection.getNode();
                    var nodeName = selectedElm.nodeName;
                    var modify = null;

                    if(nodeName.toUpperCase() === "DIV" && selectedElm.classList.contains("embeditem")){
                        // See if we can go and get the attributes
                        var embedUrl = editor.dom.getAttrib(selectedElm, "data-embed-url");
                        var embedWidth = editor.dom.getAttrib(selectedElm, "data-embed-width");
                        var embedHeight = editor.dom.getAttrib(selectedElm, "data-embed-height");
                        var embedConstrain = editor.dom.getAttrib(selectedElm, "data-embed-constrain");

                        modify = {
                            url: embedUrl,
                            width: parseInt(embedWidth) || 0,
                            height: parseInt(embedHeight) || 0,
                            constrain: embedConstrain
                        };
                    }

                    if (callback) {
                        angularHelper.safeApply($rootScope, function() {
                            // pass the active element along so we can retrieve it later
                            callback(selectedElm, modify);
                        });
                    }
                }
            });
        },

        insertEmbeddedMediaInEditor: function (editor, embed, activeElement) {
            // Wrap HTML preview content here in a DIV with non-editable class of .mceNonEditable
            // This turns it into a selectable/cutable block to move about
            var wrapper = tinymce.activeEditor.dom.create('div',
                {
                    'class': 'mceNonEditable embeditem',
                    'data-embed-url': embed.url,
                    'data-embed-height': embed.height,
                    'data-embed-width': embed.width,
                    'data-embed-constrain': embed.constrain,
                    'contenteditable': false
                },
                embed.preview);

            // Only replace if activeElement is an Embed element.
            if (activeElement && activeElement.nodeName.toUpperCase() === "DIV" && activeElement.classList.contains("embeditem")){
                activeElement.replaceWith(wrapper); // directly replaces the html node
            } else {
                editor.selection.setNode(wrapper);
            }
        },


        createAceCodeEditor: function(editor, callback){

            editor.addButton("ace", {
                icon: "code",
                tooltip: "View Source Code",
                onclick: function(){
                    if (callback) {
                        angularHelper.safeApply($rootScope, function() {
                            callback();
                        });
                    }
                }
            });

        },

        /**
         * @ngdoc method
         * @name umbraco.services.tinyMceService#createMediaPicker
         * @methodOf umbraco.services.tinyMceService
         *
         * @description
         * Creates the umbraco insert media tinymce plugin
         *
         * @param {Object} editor the TinyMCE editor instance
         */
        createMediaPicker: function (editor, callback) {
            editor.addButton('umbmediapicker', {
                icon: 'custom icon-picture',
                tooltip: 'Media Picker',
                stateSelector: 'img[data-udi]',
                onclick: function () {

                  var selectedElm = editor.selection.getNode(),
                    currentTarget;

                    if (selectedElm.nodeName === 'IMG') {
                        var img = $(selectedElm);

                        var hasUdi = img.attr("data-udi") ? true : false;
                        var hasDataTmpImg = img.attr("data-tmpimg") ? true : false;

                        currentTarget = {
                            altText: img.attr("alt"),
                            url: img.attr("src"),
                            caption: img.attr('data-caption')
                        };

                        if (hasUdi) {
                            currentTarget["udi"] = img.attr("data-udi");
                        } else {
                            currentTarget["id"] = img.attr("rel");
                        }

                        if(hasDataTmpImg){
                            currentTarget["tmpimg"] = img.attr("data-tmpimg");
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
        /**
         * @ngdoc method
         * @name umbraco.services.tinyMceService#insetMediaInEditor
         * @methodOf umbraco.services.tinyMceService
         *
         * @description
         * Inserts the image element in tinymce plugin
         *
         * @param {Object} editor the TinyMCE editor instance
         */
        insertMediaInEditor: function (editor, img) {
            if (img) {
                // We need to create a NEW DOM <img> element to insert
                // setting an attribute of ID to __mcenew, so we can gather a reference to the node, to be able to update its size accordingly to the size of the image.
                var data = {
                    alt: img.altText || "",
                    src: (img.url) ? img.url : "nothing.jpg",
                    id: "__mcenew",
                    "data-udi": img.udi,
                    "data-caption": img.caption
                };
                var newImage = editor.dom.createHTML('img', data);
                var parentElement = editor.selection.getNode().parentElement;
                    
                if (img.caption) {
                    var figCaption = editor.dom.createHTML('figcaption', {}, img.caption);
                    var combined = newImage + figCaption;
                        
                    if (parentElement.nodeName !== 'FIGURE') {
                        var fragment = editor.dom.createHTML('figure', {}, combined);
                        editor.selection.setContent(fragment);
                    }
                    else {
                        parentElement.innerHTML = combined;
                    }
                }
                else {
                    //if caption is removed, remove the figure element
                    if (parentElement.nodeName === 'FIGURE') {
                        parentElement.parentElement.innerHTML = newImage;
                    }
                    else {
                        editor.selection.setContent(newImage);
                    }
                }
                  
                // Using settimeout to wait for a DoM-render, so we can find the new element by ID.
                $timeout(function () {

                    var imgElm = editor.dom.get("__mcenew");
                    editor.dom.setAttrib(imgElm, "id", null);

                    // When image is loaded we are ready to call sizeImageInEditor.
                    var onImageLoaded = function() {
                        sizeImageInEditor(editor, imgElm, img.url);
                        editor.fire("Change");
                    }

                    // Check if image already is loaded.
                    if(imgElm.complete === true) {
                        onImageLoaded();
                    } else {
                        imgElm.onload = onImageLoaded;
                    }

                });
                
            }
        },

        /**
         * @ngdoc method
         * @name umbraco.services.tinyMceService#createUmbracoMacro
         * @methodOf umbraco.services.tinyMceService
         *
         * @description
         * Creates the insert umbraco macro tinymce plugin
         *
         * @param {Object} editor the TinyMCE editor instance
         */
        createInsertMacro: function (editor, callback) {

            let self = this;
            let activeMacroElement = null; //track an active macro element

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

            /** when the contents load we need to find any macros declared and load in their content */
            editor.on("SetContent", function (o) {

                //get all macro divs and load their content
                $(editor.dom.select(".umb-macro-holder.mceNonEditable")).each(function () {
                    self.loadMacroContent($(this), null, editor);
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

                    let ctrl = this;

                    /**
                     * Check if the macro is currently selected and toggle the menu button
                     */
                    function onNodeChanged(evt) {

                        //set our macro button active when on a node of class umb-macro-holder
                        activeMacroElement = getRealMacroElem(evt.element);

                        //set the button active/inactive
                        ctrl.active(activeMacroElement !== null);
                    }

                    //NOTE: This could be another way to deal with the active/inactive state
                    //editor.on('ObjectSelected', function (e) {});

                    //set onNodeChanged event listener
                    editor.on('NodeChange', onNodeChanged);

                },

                /** The insert macro button click event handler */
                onclick: function () {

                    var dialogData = {
                        //flag for use in rte so we only show macros flagged for the editor
                        richTextEditor: true
                    };

                    //when we click we could have a macro already selected and in that case we'll want to edit the current parameters
                    //so we'll need to extract them and submit them to the dialog.
                    if (activeMacroElement) {
                        //we have a macro selected so we'll need to parse it's alias and parameters
                        var contents = $(activeMacroElement).contents();
                        var comment = _.find(contents, function (item) {
                            return item.nodeType === 8;
                        });
                        if (!comment) {
                            throw "Cannot parse the current macro, the syntax in the editor is invalid";
                        }
                        var syntax = comment.textContent.trim();
                        var parsed = macroService.parseMacroSyntax(syntax);
                        dialogData = {
                            macroData: parsed,
                            activeMacroElement: activeMacroElement //pass the active element along so we can retrieve it later
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

        insertMacroInEditor: function (editor, macroObject, activeMacroElement) {

            //Important note: the TinyMce plugin "noneditable" is used here so that the macro cannot be edited,
            // for this to work the mceNonEditable class needs to come last and we also need to use the attribute contenteditable = false
            // (even though all the docs and examples say that is not necessary)

            //put the macro syntax in comments, we will parse this out on the server side to be used
            //for persisting.
            var macroSyntaxComment = "<!-- " + macroObject.syntax + " -->";
            //create an id class for this element so we can re-select it after inserting
            var uniqueId = "umb-macro-" + editor.dom.uniqueId();
            var macroDiv = editor.dom.create('div',
                {
                    'class': 'umb-macro-holder ' + macroObject.macroAlias + " " + uniqueId + ' mceNonEditable',
                    'contenteditable': 'false'
                },
                macroSyntaxComment + '<ins>Macro alias: <strong>' + macroObject.macroAlias + '</strong></ins>');

            //if there's an activeMacroElement then replace it, otherwise set the contents of the selected node
            if (activeMacroElement) {
                activeMacroElement.replaceWith(macroDiv); //directly replaces the html node
            }
            else {
                editor.selection.setNode(macroDiv);
            }

            var $macroDiv = $(editor.dom.select("div.umb-macro-holder." + uniqueId));
            editor.setDirty(true);

            //async load the macro content
            this.loadMacroContent($macroDiv, macroObject, editor);

        },

        /** loads in the macro content async from the server */
        loadMacroContent: function ($macroDiv, macroData, editor) {

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

            // Add the contenteditable="false" attribute
            // As just the CSS class of .mceNonEditable is not working by itself?!
            // TODO: At later date - use TinyMCE editor DOM manipulation as opposed to jQuery
            $macroDiv.attr("contenteditable", "false");

            var contentId = $routeParams.id;

            //need to wrap in safe apply since this might be occuring outside of angular
            angularHelper.safeApply($rootScope, function () {
                macroResource.getMacroResultAsHtmlForEditor(macroData.macroAlias, contentId, macroData.macroParamsDictionary)
                    .then(function (htmlResult) {

                        $macroDiv.removeClass("loading");
                        htmlResult = htmlResult.trim();
                        if (htmlResult !== "") {
                            var wasDirty = editor.isDirty();
                            $ins.html(htmlResult);
                            if (!wasDirty) {
                                editor.undoManager.clear();
                            }
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

            // the editor frame catches Ctrl+S and handles it with the system save dialog
            // - we want to handle it in the content controller, so we'll emit an event instead
            editor.addShortcut('Ctrl+S', '', function () {
                angularHelper.safeApply($rootScope, function() {
                    eventsService.emit("rte.shortcut.save");
                });
            });

            editor.addShortcut('Ctrl+P', '', function () {
                angularHelper.safeApply($rootScope, function () {
                    eventsService.emit("rte.shortcut.saveAndPublish");
                });
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
                    var selectedContent = editor.selection.getContent();
                    // If there is no selected content, we can't insert a link
                    // as TinyMCE needs selected content for this, so instead we
                    // create a new dom element and insert it, using the chosen
                    // link name as the content.
                    if (selectedContent !== "") {
                        editor.execCommand('mceInsertLink', false, createElemAttributes());
                    } else {
                        // Using the target url as a fallback, as href might be confusing with a local link
                        var linkContent = typeof target.name !== "undefined" && target.name !== "" ? target.name : target.url
                        var domElement = editor.dom.createHTML("a", createElemAttributes(), linkContent);
                        editor.execCommand('mceInsertContent', false, domElement);
                    }
                }
            }

            if (!href && !target.anchor) {
                editor.execCommand('unlink');
                return;
            }

            //if we have an id, it must be a locallink:id
            if (id) {

                href = "/{localLink:" + id + "}";

                insertLink();
                return;
            }

            if (!href) {
                href = "";
            }

            // Is email and not //user@domain.com and protocol (e.g. mailto:, sip:) is not specified
            if (href.indexOf('@') > 0 && href.indexOf('//') === -1 && href.indexOf(':') === -1) {
                // assume it's a mailto link
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

            //we can't pin the toolbar if this doesn't exist (i.e. when in distraction free mode)
            if (!editor.editorContainer) {
                return;
            }

            var tinyMce = $(editor.editorContainer);
            var toolbar = tinyMce.find(".mce-toolbar");
            var toolbarHeight = toolbar.height();
            var tinyMceRect = editor.editorContainer.getBoundingClientRect();
            var tinyMceTop = tinyMceRect.top;
            var tinyMceBottom = tinyMceRect.bottom;
            var tinyMceWidth = tinyMceRect.width;

            var tinyMceEditArea = tinyMce.find(".mce-edit-area");

            // set padding in top of mce so the content does not "jump" up
            tinyMceEditArea.css("padding-top", toolbarHeight);

            if (tinyMceTop < 177 && ((177 + toolbarHeight) < tinyMceBottom)) {
                toolbar
                    .css("position", "fixed")
                    .css("top", "177px")
                    .css("left", "auto")
                    .css("right", "auto")
                    .css("width", tinyMceWidth);
            } else {
                toolbar
                    .css("position", "absolute")
                    .css("left", "")
                    .css("right", "")
                    .css("top", "")
                    .css("width", "");
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

            // force TinyMCE to load plugins/themes from minified files (see http://archive.tinymce.com/wiki.php/api4:property.tinymce.suffix.static)
            args.editor.suffix = ".min";

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

                if(args.model.value === args.editor.getContent()) {
                    return;
                }

                //stop watching before we update the value
                stopWatch();
                angularHelper.safeApply($rootScope, function () {
                    args.model.value = args.editor.getContent();

                    //make the form dirty manually so that the track changes works, setting our model doesn't trigger
                    // the angular bits because tinymce replaces the textarea.
                    if (args.currentForm) {
                        args.currentForm.$setDirty();
                    }
                    // With complex validation we need to set a input field to dirty, not the form. but we will keep the old code for backwards compatibility.
                    if (args.currentFormInput) {
                        args.currentFormInput.$setDirty();
                    }
                });

                //re-watch the value
                startWatch();
            }

            // If we can not find the insert image/media toolbar button
            // Then we need to add an event listener to the editor
            // That will update native browser drag & drop events
            // To update the icon to show you can NOT drop something into the editor
            
            var toolbarItems = args.editor.settings.toolbar === false ? [] : args.editor.settings.toolbar.split(" ");
            if(isMediaPickerEnabled(toolbarItems) === false){
                // Wire up the event listener
                args.editor.on('dragend dragover draggesture dragdrop drop drag', function (e) {
                    e.preventDefault();
                    e.dataTransfer.effectAllowed = "none";
                    e.dataTransfer.dropEffect = "none";
                    e.stopPropagation();
                });
            }

            args.editor.on('SetContent', function (e) {
                var content = e.content;

                // Upload BLOB images (dragged/pasted ones)
                // find src attribute where value starts with `blob:`
                // search is case-insensitive and allows single or double quotes
                if(content.search(/src=["']blob:.*?["']/gi) !== -1){
                    args.editor.uploadImages(function(data) {
                        // Once all images have been uploaded
                        data.forEach(function(item) {
                            // Select img element
                            var img = item.element;

                            // Get img src
                            var imgSrc = img.getAttribute("src");
                            var tmpLocation = localStorageService.get(`tinymce__${imgSrc}`)

                            // Select the img & add new attr which we can search for
                            // When its being persisted in RTE property editor
                            // To create a media item & delete this tmp one etc
                            tinymce.activeEditor.$(img).attr({ "data-tmpimg": tmpLocation });

                            // Resize the image to the max size configured
                            // NOTE: no imagesrc passed into func as the src is blob://...
                            // We will append ImageResizing Querystrings on perist to DB with node save
                            sizeImageInEditor(args.editor, img);
                        });


                    });

                    // Get all img where src starts with blob: AND does NOT have a data=tmpimg attribute
                    // This is most likely seen as a duplicate image that has already been uploaded
                    // editor.uploadImages() does not give us any indiciation that the image been uploaded already
                    var blobImageWithNoTmpImgAttribute = args.editor.dom.select("img[src^='blob:']:not([data-tmpimg])");

                    //For each of these selected items
                    blobImageWithNoTmpImgAttribute.forEach(imageElement => {
                        var blobSrcUri = args.editor.dom.getAttrib(imageElement, "src");

                        // Find the same image uploaded (Should be in LocalStorage)
                        // May already exist in the editor as duplicate image
                        // OR added to the RTE, deleted & re-added again
                        // So lets fetch the tempurl out of localstorage for that blob URI item
                        var tmpLocation = localStorageService.get(`tinymce__${blobSrcUri}`)

                        if(tmpLocation){
                            sizeImageInEditor(args.editor, imageElement);
                            args.editor.dom.setAttrib(imageElement, "data-tmpimg", tmpLocation);
                        }
                    });

                }

                if(Umbraco.Sys.ServerVariables.umbracoSettings.sanitizeTinyMce === true){
                    /** prevent injecting arbitrary JavaScript execution in on-attributes. */
                    const allNodes = Array.prototype.slice.call(args.editor.dom.doc.getElementsByTagName("*"));
                    allNodes.forEach(node => {
                        for (var i = 0; i < node.attributes.length; i++) {
                            if(node.attributes[i].name.indexOf("on") === 0) {
                                node.removeAttribute(node.attributes[i].name)
                            }
                        }
                    });
                }

            });

            args.editor.on('init', function (e) {

                if (args.model.value) {
                    args.editor.setContent(args.model.value);
                }

                //enable browser based spell checking
                args.editor.getBody().setAttribute('spellcheck', true);


                /** Setup sanitization for preventing injecting arbitrary JavaScript execution in attributes:
                 * https://github.com/advisories/GHSA-w7jx-j77m-wp65
                 * https://github.com/advisories/GHSA-5vm8-hhgr-jcjp
                 */
                const uriAttributesToSanitize = ['src', 'href', 'data', 'background', 'action', 'formaction', 'poster', 'xlink:href'];
                const parseUri = function() {
                    // Encapsulated JS logic.
                    const safeSvgDataUrlElements = [ 'img', 'video' ];
                    const scriptUriRegExp = /((java|vb)script|mhtml):/i;
                    const trimRegExp = /[\s\u0000-\u001F]+/g;
                    const isInvalidUri = (uri, tagName) => {
                        if (/^data:image\//i.test(uri)) {
                            return safeSvgDataUrlElements.indexOf(tagName) !== -1 && /^data:image\/svg\+xml/i.test(uri);
                        } else {
                            return /^data:/i.test(uri);
                        }
                    };

                    return function parseUri(uri, tagName) {
                        uri = uri.replace(trimRegExp, '');
                        try {
                            // Might throw malformed URI sequence
                            uri = decodeURIComponent(uri);
                        } catch (ex) {
                            // Fallback to non UTF-8 decoder
                            uri = unescape(uri);
                        }

                        if (scriptUriRegExp.test(uri)) {
                            return;
                        }

                        if (isInvalidUri(uri, tagName)) {
                            return;
                        }

                        return uri;
                    }
                }();

                if(Umbraco.Sys.ServerVariables.umbracoSettings.sanitizeTinyMce === true){
                    args.editor.serializer.addAttributeFilter(uriAttributesToSanitize, function (nodes) {
                        nodes.forEach(function(node) {
                            node.attributes.forEach(function(attr) {
                                const attrName = attr.name.toLowerCase();
                                if(uriAttributesToSanitize.indexOf(attrName) !== -1) {
                                    attr.value = parseUri(attr.value, node.name);
                                }
                            });
                        });
                    });
                }

                //start watching the value
                startWatch();
            });

            args.editor.on('Change', function (e) {
                syncContent();
            });
            args.editor.on('Keyup', function (e) {
                syncContent();
            });

            //when we leave the editor (maybe)
            args.editor.on('blur', function (e) {
                syncContent();
            });

            // When the element is removed from the DOM, we need to terminate
            // any active watchers to ensure scopes are disposed and do not leak.
            // No need to sync content as that has already happened.
            args.editor.on('remove', () => stopWatch());

            args.editor.on('ObjectResized', function (e) {
                var srcAttr = $(e.target).attr("src");
                var path = srcAttr.split("?")[0];
                mediaHelper.getProcessedImageUrl(path, {
                    width: e.width,
                    height: e.height,
                    mode: "max"
                }).then(function (resizedPath) {
                    $(e.target).attr("data-mce-src", resizedPath);
                });

                syncContent();
            });

            args.editor.on('Dirty', function (e) {
            	syncContent(); // Set model.value to the RTE's content
            });

            let self = this;

            //create link picker
            self.createLinkPicker(args.editor, function (currentTarget, anchorElement) {

                entityResource.getAnchors(args.model.value).then(anchorValues => {

                    const linkPicker = {
                        currentTarget: currentTarget,
                        dataTypeKey: args.model.dataTypeKey,
                        ignoreUserStartNodes: args.model.config.ignoreUserStartNodes,
                        anchors: anchorValues,
                        size: args.model.config.overlaySize,
                        submit: model => {
                            self.insertLinkInEditor(args.editor, model.target, anchorElement);
                            editorService.close();
                        },
                        close: () => {
                            editorService.close();
                        }
                    };

                    editorService.linkPicker(linkPicker);
                });

            });

            //Create the insert media plugin
            self.createMediaPicker(args.editor, function (currentTarget, userData, imgDomElement) {

                var startNodeId, startNodeIsVirtual;
                if (!args.model.config.startNodeId) {
                    if (args.model.config.ignoreUserStartNodes === true) {
                        startNodeId = -1;
                        startNodeIsVirtual = true;
                    }
                    else {
                        startNodeId = userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0];
                        startNodeIsVirtual = userData.startMediaIds.length !== 1;
                    }
                }

                var mediaPicker = {
                    currentTarget: currentTarget,
                    onlyImages: true,
                    showDetails: true,
                    disableFolderSelect: true,
                    disableFocalPoint: true,
                    startNodeId: startNodeId,
                    startNodeIsVirtual: startNodeIsVirtual,
                    dataTypeKey: args.model.dataTypeKey,
                    submit: function (model) {
                        self.insertMediaInEditor(args.editor, model.selection[0], imgDomElement);
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.mediaPicker(mediaPicker);
            });

            //Create the embedded plugin
            self.createInsertEmbeddedMedia(args.editor, function (activeElement, modify) {
                var embed = {
                    modify: modify,
                    submit: function (model) {
                        self.insertEmbeddedMediaInEditor(args.editor, model.embed, activeElement);
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
                        self.insertMacroInEditor(args.editor, macroObject, dialogData.activeMacroElement);
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.macroPicker(macroPicker);
            });

            self.createAceCodeEditor(args.editor, function () {

                // TODO: CHECK TO SEE WHAT WE NEED TO DO WIT MACROS (See code block?)
                /*
                var html = editor.getContent({source_view: true});
                html = html.replace(/<span\s+class="CmCaReT"([^>]*)>([^<]*)<\/span>/gm, String.fromCharCode(chr));
                editor.dom.remove(editor.dom.select('.CmCaReT'));
                html = html.replace(/(<div class=".*?umb-macro-holder.*?mceNonEditable.*?"><!-- <\?UMBRACO_MACRO macroAlias="(.*?)".*?\/> --> *<ins>)[\s\S]*?(<\/ins> *<\/div>)/ig, "$1Macro alias: <strong>$2</strong>$3");
                */

                var aceEditor = {
                    content: args.editor.getContent(),
                    view: 'views/propertyeditors/rte/codeeditor.html',
                    submit: function (model) {
                        args.editor.setContent(model.content);
                        args.editor.fire('Change');
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };

                editorService.open(aceEditor);
            });

        }

    };
}

angular.module('umbraco.services').factory('tinyMceService', tinyMceService);
