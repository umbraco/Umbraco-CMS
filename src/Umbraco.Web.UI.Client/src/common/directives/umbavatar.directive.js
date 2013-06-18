/**
* @ngdoc directive 
* @name umbraco.directives:login 
* @restrict E
**/
function avatarDirective() {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/directives/umb-avatar.html'        
    };
}

angular.module('umbraco.directives').directive("umbAvatar", avatarDirective);
