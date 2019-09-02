angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.RTEController",
        function ($scope, $q, assetsService, $timeout, tinyMceService, angularHelper, tinyMceAssets) {

            // TODO: A lot of the code below should be shared between the grid rte and the normal rte

            $scope.isLoading = true;

            //To id the html textarea we need to use the datetime ticks because we can have multiple rte's per a single property alias
            // because now we have to support having 2x (maybe more at some stage) content editors being displayed at once. This is because
            // we have this mini content editor panel that can be launched with MNTP.
            $scope.textAreaHtmlId = $scope.model.alias + "_" + String.CreateGuid();

            var editorConfig = $scope.model.config ? $scope.model.config.editor : null;
            if (!editorConfig || angular.isString(editorConfig)) {
                editorConfig = tinyMceService.defaultPrevalues();
            }
            //make sure there's a max image size
            if (!editorConfig.maxImageSize && editorConfig.maxImageSize !== 0) {
                editorConfig.maxImageSize = tinyMceService.defaultPrevalues().maxImageSize;
            }

            var width = editorConfig.dimensions ? parseInt(editorConfig.dimensions.width, 10) || null : null;
            var height = editorConfig.dimensions ? parseInt(editorConfig.dimensions.height, 10) || null : null;

            $scope.containerWidth = editorConfig.mode === "distraction-free" ? (width ? width : "auto") : "auto";
            $scope.containerHeight = editorConfig.mode === "distraction-free" ? (height ? height : "auto") : "auto";
            $scope.containerOverflow = editorConfig.mode === "distraction-free" ? (height ? "auto" : "inherit") : "inherit";

            var promises = [];

            //queue file loading
            tinyMceAssets.forEach(function (tinyJsAsset) {
                promises.push(assetsService.loadJs(tinyJsAsset, $scope));
            });

            //stores a reference to the editor
            var tinyMceEditor = null;

            promises.push(tinyMceService.getTinyMceEditorConfig({
                htmlId: $scope.textAreaHtmlId,
                stylesheets: editorConfig.stylesheets,
                toolbar: editorConfig.toolbar,
                mode: editorConfig.mode
            }));

            //wait for queue to end
            $q.all(promises).then(function (result) {

                var standardConfig = result[promises.length - 1];
                
                if (height !== null) {
                    standardConfig.plugins.splice(standardConfig.plugins.indexOf("autoresize"), 1);
                }
                
                //create a baseline Config to extend upon
                var baseLineConfigObj = {
                    maxImageSize: editorConfig.maxImageSize,
                    width: width,
                    height: height,
                    init_instance_callback: function(editor){
                        $scope.isLoading = false;
                    }
                };

                angular.extend(baseLineConfigObj, standardConfig);

                baseLineConfigObj.setup = function (editor) {

                    //set the reference
                    tinyMceEditor = editor;

                    //initialize the standard editor functionality for Umbraco
                    tinyMceService.initializeEditor({
                        editor: editor,
                        model: $scope.model,
                        currentForm: angularHelper.getCurrentForm($scope)
                    });

                };

                /** Loads in the editor */
                function loadTinyMce() {

                    //we need to add a timeout here, to force a redraw so TinyMCE can find
                    //the elements needed
                    $timeout(function () {
                        tinymce.DOM.events.domLoaded = true;
                        tinymce.init(baseLineConfigObj);
                    }, 200);
                }

                loadTinyMce();

                //listen for formSubmitting event (the result is callback used to remove the event subscription)
                var unsubscribe = $scope.$on("formSubmitting", function () {
                    if (tinyMceEditor !== undefined && tinyMceEditor != null && !$scope.isLoading) {
                        $scope.model.value = tinyMceEditor.getContent();
                    }
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
