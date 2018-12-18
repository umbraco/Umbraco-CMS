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
                this.activeVariant = _.find(this.content.variants, variant => {
                    return variant.active;
                });

                $scope.activeVariant = this.activeVariant;

                $scope.defaultVariant = _.find(this.content.variants, variant => {
                    return variant.language.isDefault;
                });

                $scope.unlockInvariantValue = function(property) {
                    property.unlockInvariantValue = !property.unlockInvariantValue;
                };

                $scope.$watch("tabbedContentForm.$dirty",
                    function (newValue, oldValue) {
                        if (newValue === true) {
                            $scope.content.isDirty = true;
                        }
                    });
            },
            link: function(scope) {

            },
            scope: {
                content: "="
            }
        };

        return directive;

    }
    
    angular.module('umbraco.directives').directive('umbTabbedContent', tabbedContentDirective);

})();
