/**
@ngdoc directive
@name umbraco.directives.directive:umbDrawerFooter
@restrict E
@scope

@description
Use this directive to render a drawer footer

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
    <li>{@link umbraco.directives.directive:umbDrawerHeader umbDrawerHeader}</li>
    <li>{@link umbraco.directives.directive:umbDrawerContent umbDrawerContent}</li>
</ul>

**/

(function() {
    'use strict';

    function DrawerFooterDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/application/umbdrawer/umb-drawer-footer.html'
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbDrawerFooter', DrawerFooterDirective);

})();
