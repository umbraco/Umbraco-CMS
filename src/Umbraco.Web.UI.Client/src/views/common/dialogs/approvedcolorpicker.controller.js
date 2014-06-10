angular.module("umbraco")
    .controller("Umbraco.Dialogs.ApprovedColorPickerController", function ($scope, $http, umbPropEditorHelper, assetsService) {
        assetsService.loadJs("lib/cssparser/cssparser.js")
			    .then(function () {

			        var cssPath = $scope.dialogData.cssPath;
			        $scope.cssClass = $scope.dialogData.cssClass;

			        $scope.classes = [];

			        $scope.change = function (newClass) {
			            $scope.model.value = newClass;
			        }

			        $http.get(cssPath)
                        .success(function (data) {
                            var parser = new CSSParser();
                            $scope.classes = parser.parse(data, false, false).cssRules;
                            $scope.classes.splice(0, 0, "noclass");
                        })

			        assetsService.loadCss("/App_Plugins/Lecoati.uSky.Grid/lib/uSky.Grid.ApprovedColorPicker.css");
			        assetsService.loadCss(cssPath);
			    });
});