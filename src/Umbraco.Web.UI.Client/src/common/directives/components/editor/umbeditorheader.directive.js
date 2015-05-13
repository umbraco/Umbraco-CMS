angular.module("umbraco.directives.html")
.directive('umbEditorHeader', function () {
    return {
        transclude: true,
        restrict: 'E',
        replace: true,
        scope: {
           tabs: "=",
           actions: "=",
           name: "=",
           menu: "="
       },        
       templateUrl: 'views/components/editor/umb-editor-header.html'
   };
});	