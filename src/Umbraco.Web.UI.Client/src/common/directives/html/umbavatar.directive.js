/**
* @ngdoc directive
* @name umbraco.directives.directive:umbAvatar
* @restrict E
**/
function avatarDirective() {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/directives/umb-avatar.html',
        scope: {
            name: '@',
            email: '@',
            hash: '@'
        },
        link: function(scope, element, attr, ctrl) {

            scope.$watch("hash", function (val) {
                //set the gravatar url
                scope.gravatar = "//www.gravatar.com/avatar/" + val + "?s=40";
            });
            
        }
    };
}

angular.module('umbraco.directives').directive("umbAvatar", avatarDirective);
