
/*********************************************************************************************************/
/* Global function and variable for panel/page com  */
/*********************************************************************************************************/

/* Called for every canvasdesigner-over rollover */
var refrechIntelCanvasdesigner = function (name) {

    var scope = angular.element($("#canvasdesignerPanel")).scope();

    if (scope.schemaFocus != name.toLowerCase()) {
        var notFound = true;
        $.each(scope.canvasdesignerModel.configs, function (indexConfig, config) {
            if (config.name && name.toLowerCase() == config.name.toLowerCase()) {
                scope.currentSelected = config;
            }
        });
    }

    scope.clearSelectedCategory();

    scope.closeFloatPanels();

    scope.$apply();

}

/* Called when the iframe is first loaded */
var setFrameIsLoaded = function (canvasdesignerConfig, canvasdesignerPalette) {

    var scope = angular.element($("#canvasdesignerPanel")).scope();

    scope.canvasdesignerModel = canvasdesignerConfig;
    scope.canvasdesignerPalette = canvasdesignerPalette;
    scope.enableCanvasdesigner++;
    scope.$apply();
}

/* Iframe body click */
var iframeBodyClick = function () {

    var scope = angular.element($("#canvasdesignerPanel")).scope();

    scope.closeFloatPanels();
}
