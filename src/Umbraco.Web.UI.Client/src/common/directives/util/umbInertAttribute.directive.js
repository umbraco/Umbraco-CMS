(function () {
    'use strict';

    function umbInertAttributeDirective(eventsService) {
        var directive = {
            restrict: "A", // Can only be used as an attribute
            scope: {"umbInertAttribute":"@"},
            link: function (scope, element, attrs) {
                // If the value passed to the "umbInertAttribute" is "overlay" we will add/remove the inert attribute depending on what is emitted
                if (scope.umbInertAttribute === 'infinite-overlay') {
                    var infiniteEditorClassName = 'umb-editors';

                    eventsService.on('appState.editors.open', function (name, args) {
                        // The umb-editor needs a special touch :)
                        if (element.hasClass(infiniteEditorClassName)) {
                            setTimeout(function(){
                                var children = element.children();
                                var lastChildIndex = children.length - 1;
                                var lastChild = children[lastChildIndex];

                                children.attr('inert','');
                                lastChild.removeAttribute('inert');
                            }, 100);
                        }
                        // Set the inert attribute if it's missing on the element
                        else {
                            if (!element.attr('inert')) {
                                element.attr('inert','');
                            }
                        }
                    });
    
                    eventsService.on('appState.editors.close', function (name, args) {
                        if (element.hasClass(infiniteEditorClassName)) {

                            setTimeout(function(){
                                var children = element.children();
                                var lastChildIndex = args.editors.length - 1;
                                var lastChild = children[lastChildIndex];

                                // Remove the inert atribute for the last child
                                if (lastChild) {
                                    lastChild.removeAttribute('inert');
                                    
                                    // Find the first focusable element and put focus on it
                                    setFocus(lastChild);
                                }
                            }, 500);
                        }
                        // If there are no more open editor remove the inert attribute
                        else {
                            if (args.editors.length === 0) {
                                element.removeAttr('inert');
                            }
                        }
                    });
                }

                // If the value passed to the "umbInertAttribute" is "overlay" we will add/remove the inert attribute when the overlay is toggled
                if (scope.umbInertAttribute === 'overlay'){
                    eventsService.on('appState.overlay', function (name, args) {
                        // Add the inert attribute
                        if (args !== null) {
                            element.attr('inert','');
                        }
                        // Otherwise we will remove it again
                        else {
                            element.removeAttr('inert','');
                        }
                    });
                }
            }
        };

        function setFocus(element) {
            var $element = $(element);
            var focusableElementsString = 'a[href], area[href], input:not([disabled]):not(.ng-hide), select:not([disabled]), textarea:not([disabled]), button:not([disabled]):not([tabindex="-1"]), iframe, object, embed, [tabindex="0"], [contenteditable]';
            var focusableElements = $element.find(focusableElementsString);
        
            if(focusableElements.length){
                var $firstFocusableElement = $(focusableElements[0]);

                setTimeout(function() {
                    $firstFocusableElement.removeAttr('auto-umb-focus').removeAttr('focus-on-filled').focus();
                }, 100);
            }
        }

        return directive;

    }

    angular.module('umbraco.directives').directive('umbInertAttribute', umbInertAttributeDirective);

})();
