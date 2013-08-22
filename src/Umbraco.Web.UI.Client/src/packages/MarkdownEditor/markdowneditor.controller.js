angular.module("umbraco")
.controller("My.MarkdownEditorController",
//inject umbracos assetsServce
function ($scope,assetsService) {

    //tell the assets service to load the markdown.editor libs from the markdown editors
    //plugin folder

    if($scope.model.value === null || $scope.model.value === ""){
        $scope.model.value = $scope.model.config.defaultValue;
    }

    assetsService
		.load([
			"/app_plugins/markdowneditor/lib/markdown.converter.js",
            "/app_plugins/markdowneditor/lib/markdown.sanitizer.js",
            "/app_plugins/markdowneditor/lib/markdown.editor.js"
        ])
		.then(function () {
            //this function will execute when all dependencies have loaded
            var converter2 = new Markdown.Converter();
            var editor2 = new Markdown.Editor(converter2, "-" + $scope.model.alias);
            editor2.run();
		});

    //load the seperat css for the editor to avoid it blocking our js loading TEMP HACK
    assetsService.loadCss("/app_plugins/markdowneditor/lib/markdown.css");
});