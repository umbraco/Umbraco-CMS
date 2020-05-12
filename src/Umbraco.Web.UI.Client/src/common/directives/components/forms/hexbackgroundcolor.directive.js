
/**
* @ngdoc directive
* @name umbraco.directives.directive:hexBgColor
* @restrict A
* @description Used to set a hex background color on an element, this will detect valid hex and when it is valid it will set the color, otherwise
* a color will not be set.
**/
function hexBgColor() {
    return {        
        restrict: "A",
        link: function (scope, element, attr, formCtrl) {

            // Only add inline hex background color if defined and not "true".
            if (attr.hexBgInline === undefined || (attr.hexBgInline !== undefined && attr.hexBgInline === "true")) {

                var origColor = null;
                if (attr.hexBgOrig) {
                    // Set the orig based on the attribute if there is one.
                    origColor = attr.hexBgOrig;
                }
            
                attr.$observe("hexBgColor", function (newVal) {
                    if (newVal) {
                        if (!origColor) {
                            // Get the orig color before changing it.
                            origColor = element.css("border-color");
                        }
                        // Validate it - test with and without the leading hash.
                        if (/^([0-9a-f]{3}|[0-9a-f]{6})$/i.test(newVal)) {
                            element.css("background-color", "#" + newVal);
                            return;
                        }
                        if (/^#([0-9a-f]{3}|[0-9a-f]{6})$/i.test(newVal)) {
                            element.css("background-color", newVal);
                            return;
                        }
                    }

                    element.css("background-color", origColor);
                });
            }
        }
    };
}
angular.module('umbraco.directives').directive("hexBgColor", hexBgColor);
