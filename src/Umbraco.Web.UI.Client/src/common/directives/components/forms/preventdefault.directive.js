/**
@ngdoc directive
@name umbraco.directives.directive:preventDefault

@description
Use this directive to prevent default action of an element. Effectively implementing <a href="https://api.jquery.com/event.preventdefault/">jQuery's preventdefault</a>

<h3>Markup example</h3>

<pre>
    <a href="https://umbraco.com" prevent-default>Don't go to Umbraco.com</a>
</pre>

**/
angular.module("umbraco.directives")
    .directive('preventDefault', function() {
        return function(scope, element, attrs) {

            var enabled = true;
            //check if there's a value for the attribute, if there is and it's false then we conditionally don't
            //prevent default.
            if (attrs.preventDefault) {
                attrs.$observe("preventDefault", function (newVal) {
                    enabled = (newVal === "false" || newVal === 0 || newVal === false) ? false : true;
                });
            }

            $(element).on("click", function (event) {
                if (event.metaKey || event.ctrlKey) {
                    return;
                }
                else {
                    if (enabled === true) {
                        event.preventDefault();
                    }
                }
            });
        };
    });
