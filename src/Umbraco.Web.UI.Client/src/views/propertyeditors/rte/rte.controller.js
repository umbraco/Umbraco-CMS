angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.RTEController",
        function ($rootScope, $scope, $q, $locale, dialogService, $log, imageHelper, assetsService, $timeout, tinyMceService, angularHelper, stylesheetResource, macroService, editorState) {

            //TODO: A lot of the code below should be shared between the grid rte and the normal rte

            $scope.isLoading = true;

            //To id the html textarea we need to use the datetime ticks because we can have multiple rte's per a single property alias
            // because now we have to support having 2x (maybe more at some stage) content editors being displayed at once. This is because
            // we have this mini content editor panel that can be launched with MNTP.
            var d = new Date();
            var n = d.getTime();
            $scope.textAreaHtmlId = $scope.model.alias + "_" + n + "_rte";

            function syncContent(editor) {
                angularHelper.safeApply($scope, function () {
                    $scope.model.value = editor.getContent();
                });
            }

            var editorConfig = $scope.model.config.editor;
            if (!editorConfig || angular.isString(editorConfig)) {
                editorConfig = tinyMceService.defaultPrevalues();
            }
            
            var promises = [];
            if (!editorConfig.maxImageSize && editorConfig.maxImageSize != 0) {
                editorConfig.maxImageSize = tinyMceService.defaultPrevalues().maxImageSize;
            }

            //queue file loading
            if (typeof tinymce === "undefined") { // Don't reload tinymce if already loaded
                promises.push(assetsService.loadJs("lib/tinymce/tinymce.min.js", $scope));
            }

            //stores a reference to the editor
            var tinyMceEditor = null;

            promises.push(tinyMceService.getTinyMceEditorConfig({
                htmlId: $scope.textAreaHtmlId,
                stylesheets: editorConfig.stylesheets,
                toolbar: editorConfig.toolbar
            }));

            //wait for queue to end
            $q.all(promises).then(function (result) {

                var standardConfig = result[promises.length - 1];

                //create a baseline Config to exten upon
                var baseLineConfigObj = {
                    height: editorConfig.dimensions.height,
                    width: editorConfig.dimensions.width,
                    maxImageSize: editorConfig.maxImageSize
                };

                angular.extend(baseLineConfigObj, standardConfig);

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
                        syncContent(editor);
                    });

                    editor.on('ObjectResized', function (e) {
                        var qs = "?width=" + e.width + "&height=" + e.height + "&mode=max";
                        var srcAttr = $(e.target).attr("src");
                        var path = srcAttr.split("?")[0];
                        $(e.target).attr("data-mce-src", path + qs);

                        syncContent(editor);
                    });

                    tinyMceService.createLinkPicker(editor, $scope, function (currentTarget, anchorElement) {
                        $scope.linkPickerOverlay = {
                            view: "linkpicker",
                            currentTarget: currentTarget,
                            anchors: tinyMceService.getAnchorNames(JSON.stringify(editorState.current.properties)),
                            show: true,
                            submit: function (model) {
                                tinyMceService.insertLinkInEditor(editor, model.target, anchorElement);
                                $scope.linkPickerOverlay.show = false;
                                $scope.linkPickerOverlay = null;
                            }
                        };
                    });

                    //Create the insert media plugin
                    tinyMceService.createMediaPicker(editor, $scope, function (currentTarget, userData) {

                        $scope.mediaPickerOverlay = {
                            currentTarget: currentTarget,
                            onlyImages: true,
                            showDetails: true,
                            disableFolderSelect: true,
                            startNodeId: userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0],
                            startNodeIsVirtual: userData.startMediaIds.length !== 1,
                            view: "mediapicker",
                            show: true,
                            submit: function (model) {
                                tinyMceService.insertMediaInEditor(editor, model.selectedImages[0]);
                                $scope.mediaPickerOverlay.show = false;
                                $scope.mediaPickerOverlay = null;
                            }
                        };

                    });

                    //Create the embedded plugin
                    tinyMceService.createInsertEmbeddedMedia(editor, $scope, function () {

                        $scope.embedOverlay = {
                            view: "embed",
                            show: true,
                            submit: function (model) {
                                tinyMceService.insertEmbeddedMediaInEditor(editor, model.embed.preview);
                                $scope.embedOverlay.show = false;
                                $scope.embedOverlay = null;
                            }
                        };

                    });


                    //Create the insert macro plugin
                    tinyMceService.createInsertMacro(editor, $scope, function (dialogData) {

                        $scope.macroPickerOverlay = {
                            view: "macropicker",
                            dialogData: dialogData,
                            show: true,
                            submit: function (model) {
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
