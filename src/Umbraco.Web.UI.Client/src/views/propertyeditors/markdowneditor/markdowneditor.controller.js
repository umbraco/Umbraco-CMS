angular.module("umbraco")
.controller("Umbraco.PropertyEditors.MarkdownEditorController",
//inject umbracos assetsServce and dialog service
function ($scope, assetsService, dialogService, $log, imageHelper) {

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

		    //this function will execute when all dependencies have loaded
		    var converter2 = new Markdown.Converter();
		    var editor2 = new Markdown.Editor(converter2, "-" + $scope.model.alias);
		    editor2.run();

		    //subscribe to the image dialog clicks
		    editor2.hooks.set("insertImageDialog", function (callback) {


		        dialogService.mediaPicker({ callback: function (data) {
		            $(data.selection).each(function (i, item) {
		                var imagePropVal = imageHelper.getImagePropertyValue({ imageModel: item, scope: $scope });
		                callback(imagePropVal);
		            });
		        }
		        });

		        return true; // tell the editor that we'll take care of getting the image url
		    });

		});

    //load the seperat css for the editor to avoid it blocking our js loading TEMP HACK
    assetsService.loadCss("lib/markdown/markdown.css");
});