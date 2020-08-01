(function() {
    'use strict';

    function FocusLock($timeout, eventsService) {

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
            var target = element[0];
            var focusableElements;
            var firstFocusableElement;
            var lastFocusableElement;
            
            // List of elements that can be focusable within the focus lock
            var focusableElementsSelector = 'a[href]:not([disabled]):not(.ng-hide), button:not([disabled]):not(.ng-hide), textarea:not([disabled]):not(.ng-hide), input:not([disabled]):not(.ng-hide), select:not([disabled]):not(.ng-hide)';
            var bodyElement = document.querySelector('body');

            function getFocusableElements() {
                focusableElements = target.querySelectorAll(focusableElementsSelector);
                firstFocusableElement = focusableElements[0];
                lastFocusableElement = focusableElements[focusableElements.length -1];
                
                return focusableElements;
            }

            function handleKeydown() {
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
            }
            
            function observeDomChanges() {
                // Watch for DOM changes - so we can refresh the focusable elements if an element 
                // changes from being disabled to being enabled for instance
                var observer = new MutationObserver(domChange);

                // Options for the observer (which mutations to observe)
                var config = { attributes: true, attributeOldValue: true, childList: true, subtree: true};

                function domChange(mutationsList) {
                    getFocusableElements();

                    for (var mutation of mutationsList) {

                        // Look at the attributes - If the disabled attribute changes we call the getFocusableElements method
                        // ensuring the enabled element can be tabbed into
                        if (mutation.type === 'attributes') {

                            if(mutation.attributeName === 'disabled') {
                                getFocusableElements();
                            }
                        }
                    }
                }

                // Start observing the target node for configured mutations
                observer.observe(target, config);
            }

            function onInit() {
                $timeout(function() {
                    getFocusableElements();

                    if(focusableElements.length > 0) {

                        observeDomChanges();

                        var defaultFocusedElement = getAutoFocusElement(focusableElements);
    
                        // We need to add the tabbing-active class in order to highlight the focused button since the default style is
                        // outline: none; set in the stylesheet specifically
                        bodyElement.classList.add('tabbing-active');
        
                        // If there is no default focused element put focus on the first focusable element in the nodelist
                        if(defaultFocusedElement === null ){
                            firstFocusableElement.focus();
                        }
        
                        //  Handle keydown
                        target.addEventListener('keydown', handleKeydown);
                    }

                }, 500);
            }

            onInit();

            // Reinitialize the onInit() method if it was not the last editor that was closed
            eventsService.on('appState.editors.close', (event, args) => {
                if(args && args.editors.length !== 0) {
                    onInit();
                }
            });

            // Remove the event listener
            scope.$on('$destroy', function () {
                target.removeEventListener('keydown', handleKeydown);
            });
        }

        var directive = {
            restrict: 'A',
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbFocusLock', FocusLock);

})();
