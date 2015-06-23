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

.directive('onDragEnter', function () {
    return function (scope, elm, attrs) {
        elm.bind("dragenter", function () {
            scope.$apply(attrs.onDragEnter);
        });
    };
})

.directive('onDragLeave', function () {
    return function (scope, elm, attrs) {
        elm.bind("dragleave", function (event) {

            var rect = this.getBoundingClientRect();
            var getXY = function getCursorPosition(event) {
                var x, y;

                if (typeof event.clientX === 'undefined') {
                    // try touch screen
                    x = event.pageX + document.documentElement.scrollLeft;
                    y = event.pageY + document.documentElement.scrollTop;
                } else {
                    x = event.clientX + document.body.scrollLeft + document.documentElement.scrollLeft;
                    y = event.clientY + document.body.scrollTop + document.documentElement.scrollTop;
                }

                return { x: x, y : y };
            };

            var e = getXY(event.originalEvent);

            // Check the mouseEvent coordinates are outside of the rectangle
            if (e.x > rect.left + rect.width - 1 || e.x < rect.left || e.y > rect.top + rect.height - 1 || e.y < rect.top) {
                scope.$apply(attrs.onDragLeave);
            }
        });
    };
})

.directive('onDragOver', function () {
    return function (scope, elm, attrs) {
        elm.bind("dragover", function () {
            scope.$apply(attrs.onDragOver);
        });
    };
})

.directive('onDragStart', function () {
    return function (scope, elm, attrs) {
        elm.bind("dragstart", function () {
            scope.$apply(attrs.onDragStart);
        });
    };
})

.directive('onDragEnd', function () {
    return function (scope, elm, attrs) {
        elm.bind("dragend", function () {
            scope.$apply(attrs.onDragEnd);
        });
    };
})

.directive('onDrop', function () {
    return function (scope, elm, attrs) {
        elm.bind("drop", function () {
            scope.$apply(attrs.onDrop);
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
        }); // Temp removal of 1 sec timeout to prevent bug where overlay does not open. We need to find a better solution.

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