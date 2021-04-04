(function() {
    'use strict';

    function FocusLock($timeout) {

        function getAutoFocusElement (elements) {
            var elmentWithAutoFocus = null;

            elements.forEach((element) => {
                if(element.getAttribute('umb-auto-focus') === 'true') {
                    elmentWithAutoFocus = element;
                }
            });

            return elmentWithAutoFocus;
        }

        function link(scope, element) {

            function onInit() {
                // List of elements that can be focusable within the focus lock
                var focusableElementsSelector = 'a[href]:not([disabled]), button:not([disabled]), textarea:not([disabled]), input:not([disabled]), select:not([disabled])';
                var bodyElement = document.querySelector('body');
                
                $timeout(function() {
                    var target = element[0];
    
                    var focusableElements = target.querySelectorAll(focusableElementsSelector);
                    var defaultFocusedElement = getAutoFocusElement(focusableElements);
                    var firstFocusableElement = focusableElements[0];
                    var lastFocusableElement = focusableElements[focusableElements.length -1];
    
                    // If there is no default focused element put focus on the first focusable element in the nodelist
                    if(defaultFocusedElement === null ){
                        firstFocusableElement.focus();
                    }
    
                    target.addEventListener('keydown', function(event){
                        var isTabPressed = (event.key === 'Tab' || event.keyCode === 9);
            
                        if (!isTabPressed){
                            return;
                        }
            
                        // If shift + tab key
                        if(event.shiftKey){
                            // Set focus on the last focusable element if shift+tab are pressed meaning we go backwards
                            if(document.activeElement === firstFocusableElement){
                                lastFocusableElement.focus();
                                event.preventDefault();
                            }
                        }
                        // Else only the tab key is pressed
                        else{
                            // Using only the tab key we set focus on the first focusable element mening we go forward
                            if (document.activeElement === lastFocusableElement) {
                                firstFocusableElement.focus();
                                event.preventDefault();
                            }
                        }
                    });
                }, 250);
            }

            onInit();
        }

        var directive = {
            restrict: 'A',
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbFocusLock', FocusLock);

})();
