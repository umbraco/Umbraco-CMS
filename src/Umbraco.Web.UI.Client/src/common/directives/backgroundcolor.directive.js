
/**
* @ngdoc directive
* @name umbraco.directives.directive:bgColor
* @restrict A
* @description Used to set a background color on an element without any validation.
**/
function bgColor(assetsService) {
    return {
        restrict: "A",
        link: function (scope, element, attr, formCtrl) {

            var origColor = null;
            if (attr.bgOrig) {
                //set the orig based on the attribute if there is one
                origColor = attr.bgOrig;
            }

            attr.$observe("bgColor", function (newVal) {
                if (newVal) {
                    if (!origColor) {
                        //get the orig color before changing it
                        origColor = element.css("background-color");
                    }

                    assetsService.loadJs("lib/tinycolor/tinycolor.js").then(function () {
                        var t = tinycolor(newVal);
                        if (t.ok) {
                            element.css("background-color", t.toString());
                            return;
                        }
                    });
                }
                element.css("background-color", origColor);
            });
        }
    };
}
angular.module('umbraco.directives').directive("bgColor", bgColor);