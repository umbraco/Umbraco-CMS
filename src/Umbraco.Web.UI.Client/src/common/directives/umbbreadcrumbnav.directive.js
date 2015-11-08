/**
 * @ngdoc directive
 * @name umbraco.directives.directive:umbBreadcrumbNav
 * @function
 * @description
 * A breadcrumb navigation directive that shows linked ancestors.
 * 
 * @restrict E
 */
function breadcrumbNavDirective(entityResource, $compile) {

    // Workaround since ng-if won't work at the root level of the directive.
    function useEmptyMarkup(scope, element) {
        var newElement = $compile("")(scope);
        element.replaceWith(newElement);
    }

    return {
        restrict: "E", // restrict to an element
        replace: true,//TODO: Restore comment.
        templateUrl: 'views/directives/umb-breadcrumb-nav.html',
        scope: {
            nodeId: '=',
            kind: '=' // Kind can be "content" or "media".
        },
        link: function (scope, element, attrs, ctrl) {

            // Fetch all ancestors.
            var entityType = scope.kind === "content" ? "document" : "media";
            if (scope.nodeId) {
                entityResource
                    .getAncestors(scope.nodeId, entityType)
                    .then(function (anc) {
                        if (anc && anc.length > 1) {
                            scope.ancestors = anc;
                        } else {
                            useEmptyMarkup(scope, element);
                        }
                    });
            } else {
                useEmptyMarkup(scope, element);
            }

            // Set path for editing the entity.
            scope.editBasePath = "#/" + scope.kind + "/" + scope.kind + "/edit/";

        }
    };

}

angular.module('umbraco.directives').directive("umbBreadcrumbNav", breadcrumbNavDirective);