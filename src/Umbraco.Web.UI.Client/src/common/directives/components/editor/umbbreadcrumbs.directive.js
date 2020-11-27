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
            entity-type="content"
            on-open="clickBreadcrumb(ancestor)">
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

            $scope.clickBreadcrumb = function(ancestor) {
                // manipulate breadcrumb display
            }
        }

        angular.module("umbraco").controller("My.Controller", Controller);
    })();
</pre>

@param {array} ancestors Array of ancestors
@param {string} entityType The content entity type (member, media, content).
@param {callback=} onOpen Function callback when an ancestor is clicked. This will override the default link behaviour.
**/

(function () {
    'use strict';

    function BreadcrumbsDirective($location, navigationService) {

        function link(scope, el, attr, ctrl) {

            scope.allowOnOpen = false;

            scope.open = function(ancestor) {
                if(scope.onOpen && scope.allowOnOpen) {
                    scope.onOpen({'ancestor': ancestor});
                }
            };

            scope.openPath = function (ancestor, event) {
                // targeting a new tab/window?
                if (event.ctrlKey || 
                    event.shiftKey ||
                    event.metaKey || // apple
                    (event.button && event.button === 1) // middle click, >IE9 + everyone else
                ) {
                    // yes, let the link open itself
                    return;
                }
                event.stopPropagation();
                event.preventDefault();

                var path = scope.pathTo(ancestor);
                $location.path(path);
                navigationService.clearSearch(["cculture", "csegment"]);
            }

            scope.pathTo = function (ancestor) {
                return "/" + scope.entityType + "/" + scope.entityType + "/edit/" + ancestor.id;
            }

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
                forNewEntity: "=",
                entityType: "@",
                onOpen: "&"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbBreadcrumbs', BreadcrumbsDirective);

})();
