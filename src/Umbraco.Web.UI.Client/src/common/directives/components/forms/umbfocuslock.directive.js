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
            var infiniteEditorsWrapper;
            var infiniteEditors;
            var disconnectObserver = false;
            
            // List of elements that can be focusable within the focus lock
            var focusableElementsSelector = 'a[href]:not([disabled]):not(.ng-hide), button:not([disabled]):not(.ng-hide), textarea:not([disabled]):not(.ng-hide), input:not([disabled]):not(.ng-hide), select:not([disabled]):not(.ng-hide)';
            var bodyElement = document.querySelector('body');

            function getDomNodes(){
                infiniteEditorsWrapper = document.querySelector('.umb-editors');
                infiniteEditors = Array.from(infiniteEditorsWrapper.querySelectorAll('.umb-editor'));
            }

            function getFocusableElements(targetElm) {
                var elm = targetElm ? targetElm : target;
                focusableElements = elm.querySelectorAll(focusableElementsSelector);
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
            
            function observeDomChanges(init) {
                var targetToObserve = init ? document.querySelector('.umb-editors') : target;
                // Watch for DOM changes - so we can refresh the focusable elements if an element 
                // changes from being disabled to being enabled for instance
                var observer = new MutationObserver(domChange);

                // Options for the observer (which mutations to observe)
                var config = { attributes: true, childList: true, subtree: true};

                function domChange() {
                    getFocusableElements();
                }

                // Start observing the target node for configured mutations
                observer.observe(targetToObserve, config);

                // Disconnect observer
                if(disconnectObserver){
                    observer.disconnect();
                }
            }

            function cleanupEventHandlers() {
                var activeEditor = infiniteEditors[infiniteEditors.length - 1];
                var inactiveEditors = infiniteEditors.filter(editor => editor !== activeEditor);

                if(inactiveEditors.length > 0) {
                    for (var index = 0; index < inactiveEditors.length; index++) {
                        var inactiveEditor = inactiveEditors[index];
                        
                        // Remove event handlers from inactive editors
                        inactiveEditor.removeEventListener('keydown', handleKeydown);
                    }
                }
                else {
                    // Remove event handlers from the active editor
                    activeEditor.removeEventListener('keydown', handleKeydown);
                }
            }

            function onInit(targetElm, delay) {
                var timeout = delay ? delay : 500;

                $timeout(() => {

                    getDomNodes();
                    
                    // Only do the cleanup if we're in infinite editing mode
                    if(infiniteEditors.length > 0){
                        cleanupEventHandlers();
                    }

                    getFocusableElements(targetElm);

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
                        else {
                            defaultFocusedElement.focus();
                        }
        
                        //  Handle keydown
                        target.addEventListener('keydown', handleKeydown);
                    }

                }, timeout);
            }

            onInit();

            // If more than one editor is still open then re-initialize otherwise remove the event listener
            scope.$on('$destroy', function () {
                // Make sure to disconnect the observer so we potentially don't end up with having many active ones
                disconnectObserver = true;

                // Pass the correct editor in order to find the focusable elements
                var newTarget = infiniteEditors[infiniteEditors.length - 2];

                if(infiniteEditors.length > 1){
                    // Passing the timeout parameter as a string on purpose to bypass the falsy value that a number would give
                    onInit(newTarget, '0');
                    return;
                }
                
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
