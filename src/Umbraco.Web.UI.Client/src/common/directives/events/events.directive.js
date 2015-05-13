/**
* @description Utillity directives for key and field events
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

.directive('onOutsideClick', function ($timeout) {
    return function (scope, element, attrs) {
        
        function oneTimeClick(event) {
                var el = event.target.nodeName;
                //ignore link and button clicks
                var els = ["INPUT","A","BUTTON"];
                if(els.indexOf(el) >= 0){return;}

                //ignore children of links and buttons
                var parents = $(event.target).parents("a,button");
                if(parents.length > 0){
                    return;
                }
                
                //ignore clicks inside this element
                if( $(element).has( $(event.target) ).length > 0 ){
                    return;
                } 

                scope.$apply(attrs.onOutsideClick);
        }

        $timeout(function(){
            $(document).on("click", oneTimeClick);

            scope.$on("$destroy", function() {
                $(document).off("click", oneTimeClick);
            }); 
        }, 1000);

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