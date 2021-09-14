//inject umbracos assetsServce and dialog service
function MarkdownEditorController($scope, $element, assetsService, editorService, $timeout) {

    //tell the assets service to load the markdown.editor libs from the markdown editors
    //plugin folder

    if ($scope.model.value === null || $scope.model.value === "") {
        $scope.model.value = $scope.model.config.defaultValue;
    }

    // create a unique ID for the markdown editor, so the button bar bindings can handle split view
    // - must be bound on scope, not scope.model - otherwise it won't work, because $scope.model is used in both sides of the split view
    $scope.editorId = $scope.model.alias + _.uniqueId("-");

    function openMediaPicker(callback) {
        var mediaPicker = {
            disableFolderSelect: true,
            submit: function(model) {
                var selectedImagePath = model.selection[0].image;
                callback(selectedImagePath);
                editorService.close();
            },
            close: function() {
                editorService.close();
            }
        };
        editorService.mediaPicker(mediaPicker);
    }

    function openLinkPicker(callback) {

        const linkPicker = {
            hideTarget: true,
            size: $scope.model.config.overlaySize,
            submit: model => {
                callback(model.target.url, model.target.name);
                editorService.close();
            },
            close: () => {
                editorService.close();
            }
        };

        editorService.linkPicker(linkPicker);
    }

    function setDirty() {
        if ($scope.modelValueForm) {
            $scope.modelValueForm.modelValue.$setDirty();
        }
    }

    assetsService
        .load([
            "lib/markdown/markdown.converter.js",
            "lib/markdown/markdown.sanitizer.js",
            "lib/markdown/markdown.editor.js"
        ])
        .then(function () {

            // we need a short delay to wait for the textbox to appear.
            setTimeout(function () {
                //this function will execute when all dependencies have loaded
                // but in the case that they've been previously loaded, we can only
                // init the md editor after this digest because the DOM needs to be ready first
                // so run the init on a timeout
                $timeout(function () {
                    $scope.markdownEditorInitComplete = false;
                    var converter2 = new Markdown.Converter();
                    var editor2 = new Markdown.Editor(converter2, "-" + $scope.editorId);
                    editor2.run();

                    //subscribe to the image dialog clicks
                    editor2.hooks.set("insertImageDialog", function (callback) {
                        openMediaPicker(callback);
                        return true; // tell the editor that we'll take care of getting the image url
                    });

                    //subscribe to the link dialog clicks
                    editor2.hooks.set("insertLinkDialog", function (callback) {
                        openLinkPicker(callback);
                        return true; // tell the editor that we'll take care of getting the link url
                    });

                    editor2.hooks.set("onPreviewRefresh", function () {
                        // We must manually update the model as there is no way to hook into the markdown editor events without exstensive edits to the library.
                        if ($scope.model.value !== $("textarea", $element).val()) {
                            if ($scope.markdownEditorInitComplete) {
                                //only set dirty after init load to avoid "unsaved" dialogue when we don't want it
                                setDirty();
                            } else {
                                $scope.markdownEditorInitComplete = true;
                            }
                            $scope.model.value = $("textarea", $element).val();
                        }
                    });

                }, 200);
            });

            // HACK: load the separate css for the editor to avoid it blocking our js loading TEMP HACK
            assetsService.loadCss("lib/markdown/markdown.css", $scope);
        })
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.MarkdownEditorController", MarkdownEditorController);
