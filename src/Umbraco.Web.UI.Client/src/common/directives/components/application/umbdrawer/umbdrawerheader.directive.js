/**
@ngdoc directive
@name umbraco.directives.directive:umbDrawerHeader
@restrict E
@scope

@description
Use this directive to render a drawer header

<h3>Markup example</h3>
<pre>
	<umb-drawer-view>
        
        <umb-drawer-header
            title="Drawer Title"
            description="Drawer description">
        </umb-drawer-header>

        <umb-drawer-content>
            <!-- Your content here -->
            <pre>{{ model | json }}</pre>
        </umb-drawer-content>

        <umb-drawer-footer>
            <!-- Your content here -->
        </umb-drawer-footer>

	</umb-drawer-view>
</pre>

<h3>Use in combination with</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbDrawerView umbDrawerView}</li>
    <li>{@link umbraco.directives.directive:umbDrawerContent umbDrawerContent}</li>
    <li>{@link umbraco.directives.directive:umbDrawerFooter umbDrawerFooter}</li>
</ul>

@param {string} title (<code>attribute</code>): Set a drawer title.
@param {string} description (<code>attribute</code>): Set a drawer description.
**/

(function() {
    'use strict';

    function DrawerHeaderDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/application/umbdrawer/umb-drawer-header.html',
            scope: {
                "title": "@?",
                "description": "@?"
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbDrawerHeader', DrawerHeaderDirective);

})();
