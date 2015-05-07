//inject umbracos assetsServce and dialog service
function MarkdownEditorController($scope, assetsService, dialogService, $timeout) {

    //tell the assets service to load the markdown.editor libs from the markdown editors
    //plugin folder

    if ($scope.model.value === null || $scope.model.value === "") {
        $scope.model.value = $scope.model.config.defaultValue;
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
                    var converter2 = new Markdown.Converter();
                    var editor2 = new Markdown.Editor(converter2, "-" + $scope.model.alias);
                    editor2.run();

                    //subscribe to the image dialog clicks
                    editor2.hooks.set("insertImageDialog", function (callback) {

                        dialogService.mediaPicker({
                            callback: function (data) {
                                callback(data.image);
                            }
                        });

                        return true; // tell the editor that we'll take care of getting the image url
                    });
                }, 200);
            });

            //load the seperat css for the editor to avoid it blocking our js loading TEMP HACK
            assetsService.loadCss("lib/markdown/markdown.css");
        })
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.MarkdownEditorController", MarkdownEditorController);