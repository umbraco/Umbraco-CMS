angular.module("umbraco.directives.html")
.directive('umbEditorMenu', function () {
    return {
       scope: {
        menu: "=",
        dimmed: "="
    },
    transclude: true,
    restrict: 'E',
    replace: true,        
    templateUrl: 'views/components/navigation/umb-editor-menu.html'
};
});