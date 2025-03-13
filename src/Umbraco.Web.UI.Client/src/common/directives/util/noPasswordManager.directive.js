/**
* @ngdoc directive
* @name umbraco.directives.directive:noPasswordManager
* @attribte
* @function
* @description
* Added attributes to tell password managers to ignore specific input fields and not inject elements via browser extensions.

* @example
* <example module="umbraco.directives">
*    <file name="index.html">
*        <input type="text" no-password-manager />
*    </file>
* </example>
**/
angular.module("umbraco.directives")
    .directive('noPasswordManager', function () {
        return {
            restrict: 'A',            
            link: function (scope, element, attrs) {                
              element
                .attr("data-lpignore", "true") // LastPass
                .attr("data-1p-ignore", ""); // 1Password
            }
        }
    });
