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

                scope.isLoading = true;

                //To id the html textarea we need to use the datetime ticks because we can have multiple rte's per a single property alias
                // because now we have to support having 2x (maybe more at some stage) content editors being displayed at once. This is because
                // we have this mini content editor panel that can be launched with MNTP.
                scope.textAreaHtmlId = scope.uniqueId + "_" + String.CreateGuid();

                let editorConfig = scope.configuration ? scope.configuration : null;
                if (!editorConfig || Utilities.isString(editorConfig)) {
                    editorConfig = tinyMceService.defaultPrevalues();
                    //for the grid by default, we don't want to include the macro or the block-picker toolbar
                    editorConfig.toolbar = _.without(editorConfig.toolbar, "umbmacro", "umbblockpicker");
                }

                //ensure the grid's global config is being passed up to the RTE, these 2 properties need to be in this format
                //since below we are just passing up `scope` as the actual model and for 2 way binding to work with `value` that
                //is the way it needs to be unless we start adding watchers. We'll just go with this for now but it's super ugly.
                scope.config = {
                    ignoreUserStartNodes: scope.ignoreUserStartNodes === "true"
                }
                scope.dataTypeKey = scope.datatypeKey; //Yes - this casing is rediculous, but it's because the var starts with `data` so it can't be `data-type-id` :/

                //stores a reference to the editor
                let tinyMceEditor = null;

                const assetPromises = [];

                //queue file loading
                tinyMceAssets.forEach(function (tinyJsAsset) {
                  assetPromises.push(assetsService.loadJs(tinyJsAsset, scope));
                });

                //wait for assets to load before proceeding
                $q.all(assetPromises)
                  .then(function () {
                    return tinyMceService.getTinyMceEditorConfig({
                      htmlId: scope.textAreaHtmlId,
                      stylesheets: editorConfig.stylesheets,
                      toolbar: editorConfig.toolbar,
                      mode: editorConfig.mode
                    })
                  })

                  // Handle additional assets loading depending on the configuration before initializing the editor
                  .then(function (tinyMceConfig) {
                    // Load the plugins.min.js file from the TinyMCE Cloud if a Cloud Api Key is specified
                    if (tinyMceConfig.cloudApiKey) {
                      return assetsService.loadJs(`https://cdn.tiny.cloud/1/${tinyMceConfig.cloudApiKey}/tinymce/${tinymce.majorVersion}.${tinymce.minorVersion}/plugins.min.js`)
                        .then(() => tinyMceConfig);
                    }

                    return tinyMceConfig;
                  })

                  //wait for config to be ready after assets have loaded
                  .then(function (standardConfig) {

                    //create a baseline Config to extend upon
                    let baseLineConfigObj = {
                        maxImageSize: editorConfig.maxImageSize
                    };

                    baseLineConfigObj.setup = function (editor) {

                        //set the reference
                        tinyMceEditor = editor;

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

                        //initialize the standard editor functionality for Umbraco
                        tinyMceService.initializeEditor({
                          editor: editor,
                          toolbar: editorConfig.toolbar,
                          model: scope,
                          // Form is found in the scope of the grid controller above us, not in our isolated scope
                          // https://github.com/umbraco/Umbraco-CMS/issues/7461
                          currentForm: angularHelper.getCurrentForm(scope.$parent)
                        });
                    };

                    Utilities.extend(baseLineConfigObj, standardConfig);

                    //we need to add a timeout here, to force a redraw so TinyMCE can find
                    //the elements needed
                    $timeout(function () {
                      tinymce.init(baseLineConfigObj);
                    }, 150);
                });

                const tabShownListener = eventsService.on("app.tabChange", function (e, args) {

                  const tabId = String(args.id);
                  const myTabId = element.closest(".umb-tab-pane").attr("rel");

                  if (tabId === myTabId) {
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
                  if (tinyMceEditor) {
                    tinyMceEditor.destroy();
                    tinyMceEditor = null;
                  }
                });
            }
        };
    });
