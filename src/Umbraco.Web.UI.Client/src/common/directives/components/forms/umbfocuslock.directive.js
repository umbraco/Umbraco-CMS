(function() {
    'use strict';

    function FocusLock($timeout, $rootScope, angularHelper) {

        // If the umb-auto-focus directive is in use we respect that by leaving the default focus on it instead of choosing the first focusable element using this function
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
            var closingEditor = false;

            if(!$rootScope.lastKnownFocusableElements){
                $rootScope.lastKnownFocusableElements = [];
            }

            $rootScope.lastKnownFocusableElements.push(document.activeElement);

            // List of elements that can be focusable within the focus lock
            var focusableElementsSelector = '[role="button"], a[href]:not([disabled]):not(.ng-hide), button:not([disabled]):not(.ng-hide), textarea:not([disabled]):not(.ng-hide), input:not([disabled]):not(.ng-hide), select:not([disabled]):not(.ng-hide)';

            // Grab the body element so we can add the tabbing class on it when needed
            var bodyElement = document.querySelector('body');

            function getDomNodes(){
                infiniteEditorsWrapper = document.querySelector('.umb-editors');
                infiniteEditors = Array.from(infiniteEditorsWrapper.querySelectorAll('.umb-editor'));
            }

            function getFocusableElements(targetElm) {
                var elm = targetElm ? targetElm : target;
                focusableElements = elm.querySelectorAll(focusableElementsSelector);
                // Set first and last focusable elements
                firstFocusableElement = focusableElements[0];
                lastFocusableElement = focusableElements[focusableElements.length - 1];
            }

            function handleKeydown(event) {
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

            function clearLastKnownFocusedElements() {
                $rootScope.lastKnownFocusableElements = [];
            }

            function setElementFocus() {
                var defaultFocusedElement = getAutoFocusElement(focusableElements);
                var lastKnownElement;

                // If an inifite editor is being closed then we reset the focus to the element that triggered the the overlay
                if(closingEditor){
                    var lastItemIndex = $rootScope.lastKnownFocusableElements.length - 1;
                    var editorInfo = infiniteEditors[0].querySelector('.editor-info');

                    // If there is only one editor open, search for the "editor-info" inside it and set focus on it
                    // This is relevant when a property editor has been selected and the editor where we selected it from
                    // is closed taking us back to the first layer
                    // Otherwise set it to the last element in the lastKnownFocusedElements array
                    if(infiniteEditors.length === 1 && editorInfo !== null){
                        lastKnownElement = editorInfo;

                        // Clear the array
                        clearLastKnownFocusedElements();
                    }
                    else {
                        lastKnownElement = $rootScope.lastKnownFocusableElements[lastItemIndex];

                        // Remove the last item from the array so we always set the correct lastKnowFocus for each layer
                        $rootScope.lastKnownFocusableElements.splice(lastItemIndex, 1);
                    }

                    // Update the lastknowelement variable here
                    closingEditor = false;
                }

                // 1st - we check for any last known element - Usually the element the trigger the opening of a new layer
                // If it exists it will receive fous
                // 2nd - We check to see if a default focus has been set using the umb-auto-focus directive. If not we set focus on
                // the first focusable element
                // 3rd - Otherwise put the focus on the default focused element
                if(lastKnownElement){
                    lastKnownElement.focus();
                }
                else if(defaultFocusedElement === null ){
                    firstFocusableElement.focus();
                }
                else {
                    defaultFocusedElement.focus();
                }
            }

            function observeDomChanges() {
                // Watch for DOM changes - so we can refresh the focusable elements if an element
                // changes from being disabled to being enabled for instance
                var observer = new MutationObserver(_.debounce(domChange, 200));

                // Options for the observer (which mutations to observe)
                var config = { attributes: true, childList: true, subtree: true};

                // Whenever the DOM changes ensure the list of focused elements is updated
                function domChange() {
                    getFocusableElements();
                }

                // Start observing the target node for configured mutations
                observer.observe(target, config);

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

            function onInit(targetElm) {

                $timeout(() => {

                    // Fetch the DOM nodes we need
                    getDomNodes();

                    // Cleanup event handlers if we're in infinite editing mode
                    if(infiniteEditors.length > 0){
                        cleanupEventHandlers();
                    }

                    getFocusableElements(targetElm);

                    if(focusableElements.length > 0) {

                        observeDomChanges();

                        setElementFocus();

                        //  Handle keydown
                        target.addEventListener('keydown', handleKeydown);
                    }

                });
            }

            scope.$on('$includeContentLoaded', () => {
                angularHelper.safeApply(scope, () => {
                    onInit();
                });
            });

            // If more than one editor is still open then re-initialize otherwise remove the event listener
            scope.$on('$destroy', function () {
                // Make sure to disconnect the observer so we potentially don't end up with having many active ones
                disconnectObserver = true;

                // Pass the correct editor in order to find the focusable elements
                var newTarget = infiniteEditors[infiniteEditors.length - 2];

                if(infiniteEditors.length > 1){
                    // Setting closing till true will let us re-apply the last known focus to then opened layer that then becomes
                    // active
                    closingEditor = true;

                    onInit(newTarget);

                    return;
                }

                // Clear lastKnownFocusableElements
                clearLastKnownFocusedElements();

                // Cleanup event handler
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


// TODO: Ensure the domObserver is NOT started when there is only one infinite overlay and it's being destroyed!
