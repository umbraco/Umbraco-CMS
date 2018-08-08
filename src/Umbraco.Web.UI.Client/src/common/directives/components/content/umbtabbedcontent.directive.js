(function () {
    'use strict';

    /** This directive is used to render out the current variant tabs and properties and exposes an API for other directives to consume  */
    function tabbedContentDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/content/umb-tabbed-content.html',
            controller: function ($scope) {
                
                //expose the property/methods for other directives to use
                this.content = $scope.content;

                $scope.$watch("tabbedContentForm.$dirty",
                    function (newValue, oldValue) {
                        if (newValue === true) {
                            $scope.content.isDirty = true;
                        }
                    });
            },
            link: function(scope) {

                function onInit() {
                    angular.forEach(scope.content.tabs, function (group) {
                        group.open = true;
                    });
                }

                onInit();

            },
            scope: {
                content: "="
            }
        };

        return directive;

    }
    
    angular.module('umbraco.directives').directive('umbTabbedContent', tabbedContentDirective);

})();
