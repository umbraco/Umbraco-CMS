(function () {
    'use strict';

    function tabbedContentDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/content/tabbed-content.html',
            controller: function ($scope) {
                
                //expose the property/methods for other directives to use
                this.content = $scope.content;

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
    
    angular.module('umbraco.directives').directive('tabbedContent', tabbedContentDirective);

})();
