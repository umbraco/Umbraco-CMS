/**
@ngdoc directive
@name umbraco.directives.directive:umbBreadcrumbs
@restrict E
@scope

@description
Use this directive to generate a list of breadcrumbs.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">
        <umb-breadcrumbs
            ng-if="vm.ancestors && vm.ancestors.length > 0"
            ancestors="vm.ancestors"
            entity-type="content">
        </umb-breadcrumbs>
    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller(myService) {

            var vm = this;
            vm.ancestors = [];

            myService.getAncestors().then(function(ancestors){
                vm.ancestors = ancestors;
            });

        }

        angular.module("umbraco").controller("My.Controller", Controller);
    })();
</pre>

@param {array} ancestors Array of ancestors
@param {string} entityType The content entity type (member, media, content).
@param {callback} Callback when an ancestor is clicked. It will override the default link behaviour.
**/

(function () {
    'use strict';

    function BreadcrumbsDirective() {

        function link(scope, el, attr, ctrl) {

            scope.allowOnOpen = false;

            scope.open = function(ancestor) {
                if(scope.onOpen && scope.allowOnOpen) {
                    scope.onOpen({'ancestor': ancestor});
                }
            };

            function onInit() {
                if ("onOpen" in attr) {
                    scope.allowOnOpen = true;
                }
            }
            
            onInit();

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-breadcrumbs.html',
            scope: {
                ancestors: "=",
                entityType: "@",
                onOpen: "&"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbBreadcrumbs', BreadcrumbsDirective);

})();
