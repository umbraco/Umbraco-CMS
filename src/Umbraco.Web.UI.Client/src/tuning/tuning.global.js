
/*********************************************************************************************************/
/* Global function and variable for panel/page com  */
/*********************************************************************************************************/

/* Called for every tuning-over rollover */
var refrechIntelTuning = function (name) {

    var scope = angular.element($("#tuningPanel")).scope();

    if (scope.schemaFocus != name.toLowerCase()) {

        var notFound = true;
        $.each(scope.tuningModel.configs, function (indexConfig, config) {
            if (config.name && name.toLowerCase() == config.name.toLowerCase()) {
                notFound = false
            }
        });

        if (notFound) {
            scope.schemaFocus = "body";
        }
        else {
            scope.schemaFocus = name.toLowerCase();
        }

    }

    scope.closeFloatPanels();

    scope.$apply();

}

/* Called when the iframe is first loaded */
var setFrameIsLoaded = function (tuningConfig, tuningPalette) {

    var scope = angular.element($("#tuningPanel")).scope();

    scope.tuningModel = tuningConfig;
    scope.tuningPalette = tuningPalette;
    scope.enableTuning++;
    scope.$apply();
}

/* Iframe body click */
var iframeBodyClick = function () {

    var scope = angular.element($("#tuningPanel")).scope();

    scope.closeFloatPanels();
}
