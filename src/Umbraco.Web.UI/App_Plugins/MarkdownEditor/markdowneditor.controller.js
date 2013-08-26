angular.module("umbraco")
.controller("My.MarkdownEditorController",
//inject umbracos assetsServce and dialog service
function ($scope,assetsService, dialogService) {

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

            //subscribe to the image dialog clicks
            editor2.hooks.set("insertImageDialog", function (callback) {
                   
                   dialogService.mediaPicker({callback: function(data){
                        $(data.selection).each(function(i, item){
                               alert(item);
                        });
                   }});

/*
                   setTimeout(function () {
                       var prompt = "We have detected that you like cats. Do you want to insert an image of a cat?";
                       if (confirm(prompt))
                           callback("http://icanhascheezburger.files.wordpress.com/2007/06/schrodingers-lolcat1.jpg")
                       else
                           callback(null);
                   }, 2000);
*/

                   return true; // tell the editor that we'll take care of getting the image url
               });
		});

    //load the seperat css for the editor to avoid it blocking our js loading TEMP HACK
    assetsService.loadCss("/app_plugins/markdowneditor/lib/markdown.css");
});