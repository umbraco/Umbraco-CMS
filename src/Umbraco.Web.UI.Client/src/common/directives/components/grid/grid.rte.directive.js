angular.module("umbraco.directives")
    .directive('gridRte', function (tinyMceService, angularHelper, assetsService, $q, $timeout, eventsService, tinyMceAssets) {
        return {
            scope: {
                uniqueId: '=',
                value: '=',
                configuration: "=", //this is the RTE configuration
                datatypeKey: '@',
                ignoreUserStartNodes: '@'
            },
            templateUrl: 'views/components/grid/grid-rte.html',
            replace: true,
            link: function (scope, element, attrs) {

                // TODO: A lot of the code below should be shared between the grid rte and the normal rte

                scope.isLoading = true;

                var promises = [];
                
                //To id the html textarea we need to use the datetime ticks because we can have multiple rte's per a single property alias
                // because now we have to support having 2x (maybe more at some stage) content editors being displayed at once. This is because
                // we have this mini content editor panel that can be launched with MNTP.
                scope.textAreaHtmlId = scope.uniqueId + "_" + String.CreateGuid();

                var editorConfig = scope.configuration ? scope.configuration : null;
                if (!editorConfig || Utilities.isString(editorConfig)) {
                    editorConfig = tinyMceService.defaultPrevalues();
                    //for the grid by default, we don't want to include the macro toolbar
                    editorConfig.toolbar = _.without(editorConfig, "umbmacro");
                }
                //make sure there's a max image size
                if (!scope.configuration.maxImageSize && scope.configuration.maxImageSize !== 0) {
                    editorConfig.maxImageSize = tinyMceService.defaultPrevalues().maxImageSize;
                }

                //ensure the grid's global config is being passed up to the RTE, these 2 properties need to be in this format
                //since below we are just passing up `scope` as the actual model and for 2 way binding to work with `value` that
                //is the way it needs to be unless we start adding watchers. We'll just go with this for now but it's super ugly.
                scope.config = {
                    ignoreUserStartNodes: scope.ignoreUserStartNodes === "true"
                }
                scope.dataTypeKey = scope.datatypeKey; //Yes - this casing is rediculous, but it's because the var starts with `data` so it can't be `data-type-id` :/

                //stores a reference to the editor
                var tinyMceEditor = null;

                //queue file loading
                tinyMceAssets.forEach(function (tinyJsAsset) {
                    promises.push(assetsService.loadJs(tinyJsAsset, scope));
                });
                promises.push(tinyMceService.getTinyMceEditorConfig({
                    htmlId: scope.textAreaHtmlId,
                    stylesheets: editorConfig.stylesheets,
                    toolbar: editorConfig.toolbar,
                    mode: editorConfig.mode
                }));

                // pin toolbar to top of screen if we have focus and it scrolls off the screen
                function pinToolbar() {
                    tinyMceService.pinToolbar(tinyMceEditor);
                }

                // unpin toolbar to top of screen
                function unpinToolbar() {
                    tinyMceService.unpinToolbar(tinyMceEditor);
                }

                $q.all(promises).then(function (result) {

                    var standardConfig = result[promises.length - 1];

                    //create a baseline Config to extend upon
                    var baseLineConfigObj = {
                        maxImageSize: editorConfig.maxImageSize
                    };

                    Utilities.extend(baseLineConfigObj, standardConfig);

                    baseLineConfigObj.setup = function (editor) {

                        //set the reference
                        tinyMceEditor = editor;

                        //initialize the standard editor functionality for Umbraco
                        tinyMceService.initializeEditor({
                            editor: editor,
                            model: scope,
                            // Form is found in the scope of the grid controller above us, not in our isolated scope
                            // https://github.com/umbraco/Umbraco-CMS/issues/7461
                            currentForm: angularHelper.getCurrentForm(scope.$parent)
                        });

                        //custom initialization for this editor within the grid
                        editor.on('init', function (e) {

                            // Used this init event - as opposed to property init_instance_callback
                            // to turn off the loader
                            scope.isLoading = false;

                            //force overflow to hidden to prevent no needed scroll
                            editor.getBody().style.overflow = "hidden";

                            $timeout(function () {
                                if (scope.value === null) {
                                    editor.focus();
                                }
                            }, 400);

                        });

                        // TODO: Perhaps we should pin the toolbar for the rte always, regardless of if it's in the grid or not?
                        // this would mean moving this code into the tinyMceService.initializeEditor

                        //when we leave the editor (maybe)
                        editor.on('blur', function (e) {
                            angularHelper.safeApply(scope, function () {
                                unpinToolbar();
                                $('.umb-panel-body').off('scroll', pinToolbar);
                            });
                        });

                        // Focus on editor
                        editor.on('focus', function (e) {
                            angularHelper.safeApply(scope, function () {
                                pinToolbar();
                                $('.umb-panel-body').on('scroll', pinToolbar);
                            });
                        });

                        // Click on editor
                        editor.on('click', function (e) {
                            angularHelper.safeApply(scope, function () {
                                pinToolbar();
                                $('.umb-panel-body').on('scroll', pinToolbar);
                            });
                        });


                    };

                    /** Loads in the editor */
                    function loadTinyMce() {

                        //we need to add a timeout here, to force a redraw so TinyMCE can find
                        //the elements needed
                        $timeout(function () {
                            tinymce.init(baseLineConfigObj);
                        }, 150, false);
                    }

                    loadTinyMce();

                    // TODO: This should probably be in place for all RTE, not just for the grid, which means
                    // this code can live in tinyMceService.initializeEditor
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
                    
                    //when the element is disposed we need to unsubscribe!
                    // NOTE: this is very important otherwise if this is part of a modal, the listener still exists because the dom
                    // element might still be there even after the modal has been hidden.
                    scope.$on('$destroy', function () {
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
