/**
@ngdoc directive
@name umbraco.directives.directive:umbIcon
@restrict E
@scope

@description
Use this directive to show an render an umbraco backoffice svg icon.

<h3>Markup example</h3>
<pre>
    <umb-icon
        name="icon-name"
        get-icon="true" // defaults to false. When true will do a http request to get the svg icon
        >
        // Empty state content
    </umb-icon>
</pre>

**/

(function () {
    'use strict';

    function UmbIconDirective($http, $sce, umbRequestHelper) {

        var directive = {
            replace: true,
            transclude: true,
            templateUrl: 'views/components/umb-icon.html',
            scope: {
                iconName: '@',
                svgString: '=?',
                getIcon: '=?'
            },
            link: function(scope) {
                
                if(scope.getIcon === true) {
                    umbRequestHelper.resourcePromise(
                        $http.get(Umbraco.Sys.ServerVariables.umbracoUrls.iconApiBaseUrl + "GetIcon?iconName=" + scope.iconName)
                        ,'Failed to retrieve icon: ' + scope.iconName)
                        .then(icon => {
                            scope.svgString = $sce.trustAsHtml(icon.SvgString);
                        });
                }
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbIcon', UmbIconDirective);

})();
