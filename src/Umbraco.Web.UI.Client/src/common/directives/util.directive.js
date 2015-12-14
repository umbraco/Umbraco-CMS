/**
* @description Utility directives for key and field events
**/
angular.module('umbraco.directives')

.directive('onKeyup', function () {
    return function (scope, elm, attrs) {
        elm.bind("keyup", function () {
            scope.$apply(attrs.onKeyup);
        });
    };
})

.directive('onKeydown', function () {
    return {
        link: function (scope, elm, attrs) {
            scope.$apply(attrs.onKeydown);
        }
    };
})

.directive('onBlur', function () {
    return function (scope, elm, attrs) {
        elm.bind("blur", function () {
            scope.$apply(attrs.onBlur);
        });
    };
})

.directive('onFocus', function () {
    return function (scope, elm, attrs) {
        elm.bind("focus", function () {
            scope.$apply(attrs.onFocus);
        });
    };
})

.directive('onRightClick',function(){
    
    document.oncontextmenu = function (e) {
       if(e.target.hasAttribute('on-right-click')) {
           e.preventDefault();
           e.stopPropagation(); 
           return false;
       }
    };  

    return function(scope,el,attrs){
        el.bind('contextmenu',function(e){
            e.preventDefault();
            e.stopPropagation();
            scope.$apply(attrs.onRightClick);
            return false;
        });
    };
});