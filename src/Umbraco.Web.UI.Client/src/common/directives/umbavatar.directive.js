/**
* @ngdoc directive 
* @name umbraco.directives:login 
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
            //set the gravatar url
            scope.gravatar = "http://www.gravatar.com/avatar/" + scope.hash + "?s=40";
        }
    };
}

angular.module('umbraco.directives').directive("umbAvatar", avatarDirective);
