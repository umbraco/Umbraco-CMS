/**
 @ngdoc service
 * @name umbraco.services.focusLockService
 *
 * @description
 * <b>Added in Umbraco 8.2</b>. Application-wide service for locking focus inside an overlday/modal when open.
 *
 */

(function () {
    "use strict";

    var domNodes = {};
    var focusableEls = 'a[href]:not([disabled]), a[ng-href]:not([disabled]), input:not([disabled]):not(.ng-hide), select:not([disabled]), textarea:not([disabled]), button:not([disabled]):not([tabindex="-1"]), [tabindex="0"]';

    /**
     * Focus lock method that needs to be called when we add overlays
     * 
     * @param {HTMLElement} elm 
     */
    function focusLock(elm){
        setTimeout(() =>{
            var focusableElsInEditor = elm.querySelectorAll(focusableEls);
            var firstFocusableEl = focusableElsInEditor[0];
            var lastFocusableEl = focusableElsInEditor[focusableElsInEditor.length -1];
            var tabKey = 9;

            elm.addEventListener('keydown', function(event){
                var isTabPressed = (event.key === 'Tab' || event.keyCode === tabKey);

                if (!isTabPressed){
                    return;
                }

                // If shift + tab key
                if(event.shiftKey){
                    // Set focus on the last focusable element if shift+tab are pressed meaning we go backwards
                    if(document.activeElement === firstFocusableEl){
                        lastFocusableEl.focus();
                        event.preventDefault();
                    }
                }
                // Else only the tab key is pressed
                else{
                    // Using only the tab key we set focus on the first focusable element mening we go forward
                    if (document.activeElement === lastFocusableEl) {
                        firstFocusableEl.focus();
                        event.preventDefault();
                    }
                }
            });

        }, 500);
    }

    function resetFocus(elm){
        setTimeout(() =>{
            elm.focus();
        }, 100);
    }

    /**
     * Helper method to fetch the DOM nodes that needs to be disabled/enabled
     */
    function getDOMNodes (){
        domNodes.appHeader = document.querySelector('.umb-app-header');
        domNodes.leftColumn = document.querySelector('#leftcolumn');
        domNodes.mainWrapper = document.querySelector('#mainwrapper');
        domNodes.editor = document.querySelector('.umb-editor');
    }

    /**
     * Helper method to enable or disable the editors so they can't be navigated to using keyboard or screen readers
     *
     * @param {Array} editors 
     * @param {Boolean} boolean 
     */
    function disableOrEnableEditors(editors, boolean){
        var editorArray = Array.from(editors)
        var currentEditorIndex = editors.length - 1;

        // Disable editors that are not current
        if(boolean){
            editorArray.forEach((editor, idx) => {
                // Disable editors that are not current
                if(idx !== currentEditorIndex){
                    editor.setAttribute('inert','');
                }
                // Add focusLock to current editor
                else{
                    focusLock(editor);
                }
            });
        }
        // Enable current editor
        else{
            editorArray.forEach((editor, idx) => {
                if(idx === currentEditorIndex){
                    var firstFocusableElement = editor.querySelector(focusableEls);

                    editor.removeAttribute('inert','');

                    resetFocus(firstFocusableElement);
                }
            });
        }
    }

    function focusLockService() {

        var service = {

            /**
             * @ngdoc function
             * @name umbraco.services.focusLockService#addInfiniteFocusLock
             * @methodOf umbraco.services.focusLockService
             * @function
             *
             * @description
             * Call this method before a new infinite overlay is activated to ensure focus is locked inside it
             *
             * @param {Array} elm array containing an element
             * @param {Number} overlays a number of overlays that are open
             *
             */
            addInfiniteFocusLock: function(elm, overlays) {
                var children = elm[0].children;

                // Get the DOM nodes we need to add/remove the inert attribute for
                getDOMNodes();

                // Disable "outer" elements once
                if(overlays === 1){
                    domNodes.leftColumn.setAttribute('inert','');
                    domNodes.appHeader.setAttribute('inert','');
                    domNodes.editor.setAttribute('inert','');
                }

                // disable child editors if they're not current
                disableOrEnableEditors(children, true);
            },

            /**
             * @ngdoc function
             * @name umbraco.services.focusLockService#removeInfiniteFocusLock
             * @methodOf umbraco.services.focusLockService
             * @function
             *
             * @description
             * Call this method when an infinite editor is closed to enable the locked elements in the DOM again
             *
             * @param {Array} elm array containing an element
             * @param {Number} overlays a number of overlays that are open
             *
             */
            removeInfiniteFocusLock: function(elm, overlays) {
                var children = elm[0].children;

                // Enable "outer" elements once
                if(overlays === 0){
                    domNodes.leftColumn.removeAttribute('inert','');
                    domNodes.appHeader.removeAttribute('inert','');
                    domNodes.editor.removeAttribute('inert','');
                }

                // enable editor if it's current
                disableOrEnableEditors(children, false);
            },

            /**
             * @ngdoc function
             * @name umbraco.services.focusLockService#addFocusLock
             * @methodOf umbraco.services.focusLockService
             * @function
             *
             * @description
             * Call this method before a new overlay is activated to ensure focus is locked inside it
             *
             */
            addFocusLock: function() {
                // Get the DOM nodes we need to add/remove the inert attribute for
                getDOMNodes();

                domNodes.mainWrapper.setAttribute('inert','');
            },

            /**
             * @ngdoc function
             * @name umbraco.services.focusLockService#removeFocusLock
             * @methodOf umbraco.services.focusLockService
             * @function
             *
             * @description
             * Call this method when an overlay is closed to enable the locked element in the DOM again
             *
             */
            removeFocusLock: function(){
                domNodes.mainWrapper.removeAttribute('inert');
            }
        };

        return service;

    }

    angular.module("umbraco.services").factory("focusLockService", focusLockService);

})();
