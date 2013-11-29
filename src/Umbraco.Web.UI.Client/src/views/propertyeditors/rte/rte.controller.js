angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.RTEController",
    function ($rootScope, $scope, $q, dialogService, $log, imageHelper, assetsService, $timeout, tinyMceService, angularHelper, stylesheetResource) {

        tinyMceService.configuration().then(function(tinyMceConfig){

            //config value from general tinymce.config file
            var validElements = tinyMceConfig.validElements;

            //These are absolutely required in order for the macros to render inline
            //we put these as extended elements because they get merged on top of the normal allowed elements by tiny mce
            var extendedValidElements = "@[id|class|style],-div[id|dir|class|align|style],ins[datetime|cite],-ul[class|style],-li[class|style]";

            var invalidElements = tinyMceConfig.inValidElements;
            var plugins = _.map(tinyMceConfig.plugins, function(plugin){ 
                                            if(plugin.useOnFrontend){
                                                return plugin.name;   
                                            }
                                        }).join(" ");
            
            var editorConfig = $scope.model.config.editor;
            if(!editorConfig || angular.isString(editorConfig)){
                editorConfig = tinyMceService.defaultPrevalues();
            }

            //config value on the data type
            var toolbar = editorConfig.toolbar.join(" | ");
            var stylesheets = [];
            var styleFormats = [];
            var await = [];

            //queue file loading
            await.push(assetsService.loadJs("lib/tinymce/tinymce.min.js", $scope));

            //queue rules loading
            angular.forEach(editorConfig.stylesheets, function(val, key){
                stylesheets.push("/css/" + val + ".css?" + new Date().getTime());
                
                await.push(stylesheetResource.getRulesByName(val).then(function(rules) {
                    angular.forEach(rules, function(rule) {
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
                        else {
                            r.block = rule.selector;
                        }

                        styleFormats.push(r);
                    });
                }));
            });


            //wait for queue to end
            $q.all(await).then(function () {
                
                /** Loads in the editor */
                function loadTinyMce() {
                    
                    //we need to add a timeout here, to force a redraw so TinyMCE can find
                    //the elements needed
                    $timeout(function () {
                        tinymce.DOM.events.domLoaded = true;
                        tinymce.init({
                            mode: "exact",
                            elements: $scope.model.alias + "_rte",
                            skin: "umbraco",
                            plugins: plugins,
                            valid_elements: validElements,
                            invalid_elements: invalidElements,
                            extended_valid_elements: extendedValidElements,
                            menubar: false,
                            statusbar: false,
                            height: editorConfig.dimensions.height,
                            width: editorConfig.dimensions.width,
                            toolbar: toolbar,
                            content_css: stylesheets.join(','),
                            relative_urls: false,
                            style_formats: styleFormats,
                            setup: function (editor) {

                                //We need to listen on multiple things here because of the nature of tinymce, it doesn't 
                                //fire events when you think!
                                //The change event doesn't fire when content changes, only when cursor points are changed and undo points
                                //are created. the blur event doesn't fire if you insert content into the editor with a button and then 
                                //press save. 
                                //We have a couple of options, one is to do a set timeout and check for isDirty on the editor, or we can 
                                //listen to both change and blur and also on our own 'saving' event. I think this will be best because a 
                                //timer might end up using unwanted cpu and we'd still have to listen to our saving event in case they clicked
                                //save before the timeout elapsed.
                                editor.on('change', function (e) {
                                    angularHelper.safeApply($scope, function () {
                                        $scope.model.value = editor.getContent();
                                    });
                                });
                                editor.on('blur', function (e) {
                                    angularHelper.safeApply($scope, function () {
                                        $scope.model.value = editor.getContent();
                                    });
                                });
                                //listen for formSubmitting event (the result is callback used to remove the event subscription)
                                var unsubscribe = $scope.$on("formSubmitting", function () {

                                    //TODO: Here we should parse out the macro rendered content so we can save on a lot of bytes in data xfer
                                    // we do parse it out on the server side but would be nice to do that on the client side before as well.
                                    $scope.model.value = editor.getContent();
                                });

                                //when the element is disposed we need to unsubscribe!
                                // NOTE: this is very important otherwise if this is part of a modal, the listener still exists because the dom 
                                // element might still be there even after the modal has been hidden.
                                $scope.$on('$destroy', function(){
                                    unsubscribe();
                                });

                                //Create the insert media plugin
                                tinyMceService.createMediaPicker(editor, $scope);

                                //Create the embedded plugin
                                tinyMceService.createInsertEmbeddedMedia(editor, $scope);

                                //Create the insert link plugin
                                tinyMceService.createLinkPicker(editor, $scope);

                                //Create the insert macro plugin
                                tinyMceService.createInsertMacro(editor, $scope);
                            }
                        });
                    }, 500);
                }
                
                loadTinyMce();

                //here we declare a special method which will be called whenever the value has changed from the server
                //this is instead of doing a watch on the model.value = faster
                $scope.model.onValueChanged = function (newVal, oldVal) {
                    //update the display val again if it has changed from the server
                    //TODO: Perhaps we don't need to re-load the whole editor, can probably just re-set the value ?
                    loadTinyMce();
                };
            });
        });

    });