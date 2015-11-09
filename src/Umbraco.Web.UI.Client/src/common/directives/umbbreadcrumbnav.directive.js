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
        var newElement = $compile('')(scope);
        element.replaceWith(newElement);
    }

    // Populates the ancestors of the node (inclusive of the node).
    function populateAncestors(scope, entityType, element) {
        entityResource
            .getAncestors(scope.nodeId, entityType)
            .then(function (anc) {
                if (anc && anc.length > 1) {
                    scope.ancestors = anc;
                } else {
                    useEmptyMarkup(scope, element);
                }
            });
    }

    // Return directive definition.
    return {
        restrict: 'E', // restrict to an element
        replace: true, // replace the html element with the template
        templateUrl: 'views/directives/umb-breadcrumb-nav.html',
        scope: {
            nodeId: '=',
            kind: '=' // Kind can be "content" or "media".
        },
        link: function (scope, element, attrs, ctrl) {

            // Fetch all ancestors.
            var entityType = scope.kind === 'content' ? 'document' : 'media';
            if (scope.nodeId) {
                populateAncestors(scope, entityType, element);
            } else {

                // Wait until the node ID is present before searching for ancestors.
                var unwatch = scope.$watch('nodeId', function (value) {
                    if (value) {
                        unwatch();
                        populateAncestors(scope, entityType, element);
                    }
                });

            }

            // Set path for editing the entity.
            scope.editBasePath = '#/' + scope.kind + '/' + scope.kind + '/edit/';

        }
    };

}

angular.module('umbraco.directives').directive('umbBreadcrumbNav', breadcrumbNavDirective);