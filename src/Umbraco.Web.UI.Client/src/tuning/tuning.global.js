
/*********************************************************************************************************/
/* Global function and variable for panel/page com  */
/*********************************************************************************************************/

/* Refresh tuning panel with selected fieds */
var refrechIntelTuning = function (schema) {

    var scope = angular.element($("#tuningPanel")).scope();

    if (scope.schemaFocus != schema.toLowerCase()) {

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
}

var setFrameIsLoaded = function (tuningParameterUrl) {
    console.info("iframe id loaded " + tuningParameterUrl);
    var scope = angular.element($("#tuningPanel")).scope();
    scope.tuningParameterUrl = tuningParameterUrl;
    scope.enableTuning++;
    scope.frameFirstLoaded = true;
    scope.$apply();
}