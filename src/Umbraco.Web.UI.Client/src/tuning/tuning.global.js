
/*********************************************************************************************************/
/* Global function and variable for panel/page com  */
/*********************************************************************************************************/

/* Called for every tuning-over rollover */
var refrechIntelTuning = function (name) {

    var scope = angular.element($("#tuningPanel")).scope();

    if (scope.schemaFocus != name.toLowerCase()) {

        var notFound = true;
        angular.forEach(scope.tuningModel.categories, function (category, key) {
            var isContainer = false;
            angular.forEach(category.sections, function (section, key) {
                angular.forEach(section.subSections, function (subSection, key) {
                    if (subSection.name && name.toLowerCase() == subSection.name.toLowerCase()) {
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
            scope.schemaFocus = name.toLowerCase();
        }

        scope.$apply();
    }
}

/* Called when the iframe is first loaded */
var setFrameIsLoaded = function (tuningParameterUrl, tuningGridList) {
    var scope = angular.element($("#tuningPanel")).scope();
    scope.tuningParameterUrl = tuningParameterUrl;
    scope.tuningGridList = tuningGridList
    scope.enableTuning++;
    scope.$apply();
}