
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

            function setBackgroundColor(color) {
                element[0].style.backgroundColor = "#" + color;
            }

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
                        // Is it a regular hex value - (#)AABBCC ?
                        var match = newVal.match(/^#?([0-9a-f]{3}|[0-9a-f]{6})$/i);
                        if (match && match.length) {
                            setBackgroundColor(match[1]);
                            return;
                        }
                        // Is it a hexa value - (#)AABBCCDD ?
                        match = newVal.match(/^#?([0-9a-f]{4}|[0-9a-f]{8})$/i);
                        if (match && match.length) {
                            setBackgroundColor(match[1]);
                            return;
                        }
                    }

                    setBackgroundColor(origColor);
                });
            }
        }
    };
}
angular.module('umbraco.directives').directive("hexBgColor", hexBgColor);
