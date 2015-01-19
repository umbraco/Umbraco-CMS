
/*********************************************************************************************************/
/* Global function and variable for panel/page com  */
/*********************************************************************************************************/

/* Called for every canvasdesigner-over click */
var onClickCanvasdesignerItem = function (schema) {

    var scope = angular.element($("#canvasdesignerPanel")).scope();

    //if (scope.schemaFocus != schema.toLowerCase()) {
    //var notFound = true;
        $.each(scope.canvasdesignerModel.configs, function (indexConfig, config) {
            if (config.schema && schema.toLowerCase() == config.schema.toLowerCase()) {
                scope.currentSelected = config;
            }
        });
    //}

    scope.clearSelectedCategory();

    scope.closeFloatPanels();

    scope.$apply();

}

/* Called for every canvasdesigner-over rollover */
var onMouseoverCanvasdesignerItem = function (name) {

    var scope = angular.element($("#canvasdesignerPanel")).scope();

    $.each(scope.canvasdesignerModel.configs, function (indexConfig, config) {
        config.highlighted = false;
        if (config.name && name.toLowerCase() == config.name.toLowerCase()) {
            config.highlighted = true;
        }
    });

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
