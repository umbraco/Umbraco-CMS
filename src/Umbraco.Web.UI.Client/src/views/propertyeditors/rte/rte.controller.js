angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.RTEController",
        function ($scope, $q, assetsService, $timeout, tinyMceService, angularHelper) {

            // TODO: A lot of the code below should be shared between the grid rte and the normal rte

            $scope.isLoading = true;

            //To id the html textarea we need to use the datetime ticks because we can have multiple rte's per a single property alias
            // because now we have to support having 2x (maybe more at some stage) content editors being displayed at once. This is because
            // we have this mini content editor panel that can be launched with MNTP.
            var d = new Date();
            var n = d.getTime();
            $scope.textAreaHtmlId = $scope.model.alias + "_" + n + "_rte";

            var editorConfig = $scope.model.config ? $scope.model.config.editor : null;
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
                toolbar: editorConfig.toolbar,
                mode: editorConfig.mode
            }));

            //wait for queue to end
            $q.all(promises).then(function (result) {

                var standardConfig = result[promises.length - 1];

                //create a baseline Config to exten upon
                var baseLineConfigObj = {
                    maxImageSize: editorConfig.maxImageSize
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

                        $scope.isLoading = false;

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
