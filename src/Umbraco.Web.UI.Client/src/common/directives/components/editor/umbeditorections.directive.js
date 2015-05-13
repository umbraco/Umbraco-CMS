angular.module("umbraco.directives.html")
.directive('umbEditorActions', function () {
    return {

    scope: {
        actions: "=",
    }, 

    transclude: true,
    restrict: 'E',
    replace: true,        
    templateUrl: 'views/components/navigation/umb-editor-actions.html'
};
});