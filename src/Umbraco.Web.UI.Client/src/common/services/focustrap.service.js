/**
 @ngdoc service
 * @name umbraco.services.focusTrapService
 *
 * @description
 * <b>Added in Umbraco 8.0.2</b>. Service for adding focus traps to overlays. Intended for internal use only to improve accesibility of overlays.
 */

function focusTrapService() {

    // TODO: Figure out how the needed DOM references can be put into an object that can be passed to the method so we don't need to get them from the DOM in each of the methods
    // TODO: Figure out how to avoid the setTimeout methods
    // TODO: Consider if some of the DOM manipulation should be done using a directive somehow...
    // TODO: Consider adding a tablock method to avoid the possibility of escaping to the browser address bar - But maybe have a discussion about this in the PR instead?...

    function addFocusTrapOverlayMode () {
        var mainWrapper = $('#mainwrapper');
    
        mainWrapper.attr('inert','');
    }
    
    function removeFocusTrapOverlayMode () {
        var mainWrapper = $('#mainwrapper');
        mainWrapper.removeAttr('inert');
    }
    
    function addFocusTrapInfiniteMode () {
        var appHeader = $('.umb-app-header');
        var leftColumn = $('#leftcolumn');
        var contentColumn = $('#contentcolumn > div:first-child');
        var editors = $('.umb-editors');
    
        // Remove focus from any interactive elements in the "appHeader", "leftColumn" and the first child in the "contentColumn"
        appHeader.attr('inert','');
        leftColumn.attr('inert','');
        contentColumn.attr('inert','');
    
        setTimeout(function(){
            toggleInert(editors);
        }, 100);
    }
    
    function removeFocusTrapInfiniteMode () {
        var appHeader = $('.umb-app-header');
        var leftColumn = $('#leftcolumn');
        var contentColumn = $('#contentcolumn > div:first-child');
    
        // Remove the inert attribute from the key elements so they're tabable once the infinite editing mode has been deactivated
        appHeader.removeAttr('inert');
        leftColumn.removeAttr('inert');
        contentColumn.removeAttr('inert');
    }
    
    function toggleInert (editors) {
        const editorChildren = editors.children();
        const lastEditorChildIndex = editorChildren.length - 1;
        const lastEditorChild = $(editorChildren[lastEditorChildIndex]);
    
        editorChildren.attr('inert','');
        lastEditorChild.removeAttr('inert');
    
        // Add focus to the first element in the potential infinite overlay that lays behind the one that we just closed
        setFocusableElement(lastEditorChild);
    }
    
    function setFocusableElement(element) {
        var focusableElementsString = 'a[href], area[href], input:not([disabled]):not(.ng-hide), select:not([disabled]), textarea:not([disabled]), button:not([disabled]):not([tabindex="-1"]), iframe, object, embed, [tabindex="0"], [contenteditable]';
        var focusableElements = element.find(focusableElementsString);
    
        if(focusableElements.length){
            var firstFocusableElement = focusableElements[0];
    
            setTimeout(function(){
                $(firstFocusableElement).removeAttr('auto-umb-focus').removeAttr('focus-on-filled').focus();
                $(firstFocusableElement).attr('auto-umb-focus');
            },100);
        }
    }

    function addFocusTrap(mode){
        if(mode === 'modal'){
            addFocusTrapOverlayMode();
        }

        if(mode === 'infinite'){
            addFocusTrapInfiniteMode();
        }
    }

    function removeFocusTrap (mode) {
        if (mode === 'modal') {
            removeFocusTrapOverlayMode();
        }

        if (mode === 'infinite') {
            removeFocusTrapInfiniteMode();
        }
    }

    // Define the service object
    var service = {
        addFocusTrap: addFocusTrap,
        removeFocusTrap: removeFocusTrap
    }

    // Return the service object
    return service;
}

angular.module('umbraco.services').factory('focusTrapService', focusTrapService);
