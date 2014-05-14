
/*********************************************************************************************************/
/* Global function and variable for panel/page com  */
/*********************************************************************************************************/

/* Refresh tuning panel with selected fieds */
var refrechIntelTuning = function (schema) {

    var scope = angular.element($("#tuningPanel")).scope();

    var notFound = true;
    angular.forEach(scope.tuningModel.categories, function (category, key) {
        var isContainer = false;
        angular.forEach(category.sections, function (section, key) {
            angular.forEach(section.subSections, function (subSection, key) {
                if (subSection.schema && schema.toLowerCase() == subSection.schema.toLowerCase()) {
                    isContainer = true;
                    notFound = false
                }

            });
        });
        if (!category.active) {
            category.active = isContainer;
        }
    });
    if (notFound) {
        scope.tuningModel.categories[0].active = true;
    }
    scope.$apply();

    if (notFound) {
        scope.schemaFocus = "body";
    }
    else {
        scope.schemaFocus = schema.toLowerCase();
    }

    scope.$apply();

}

var setFrameIsLoaded = function () {
    console.info("iframe id loaded");
    var scope = angular.element($("#tuningPanel")).scope();
    scope.frameLoaded = true;
    scope.$apply();
}