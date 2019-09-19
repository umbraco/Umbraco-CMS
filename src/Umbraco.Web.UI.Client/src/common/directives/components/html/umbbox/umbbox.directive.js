/** 
@ngdoc directive
@name umbraco.directives.directive:umbBox
@restrict E

@description
Use this directive to render an already styled empty div tag.

<h3>Markup example</h3>
<pre>
    <umb-box>
        <umb-box-header title="this is a title"></umb-box-header>
        <umb-box-content>
            // Content here
        </umb-box-content>
    </umb-box>
</pre>

<h3>Use in combination with:</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbBoxHeader umbBoxHeader}</li>
    <li>{@link umbraco.directives.directive:umbBoxContent umbBoxContent}</li>
</ul>
**/

(function(){
    'use strict';

    function BoxDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/html/umb-box/umb-box.html'
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbBox', BoxDirective);

})();