/** Used to improve accesibility ensuring that the focus is trapped inside the active overlay and also ensuring aria-hidden is set accordingly so potential screen readers don't get confused about the context either */

function focusTrapService() {

    // TODO: If possible I would like to store all the needed DOM references in an elements object, wich could then be passed to the methods so we don't need to reference and store them in each method
    // But some of the elements are not available untill the app.authenticated event happens... Not sure about the best practice around this...
    console.log('trap da focus mayn!');
    
    return {
        addFocusTrap: function(mode){
            if(mode === 'modal'){
                addFocusTrapOverlayMode();
            }

            if(mode === 'infinite'){
                addFocusTrapInfiniteMode();
            }
        },
        removeFocusTrap: function (mode) {
            if (mode === 'modal') {
                removeFocusTrapOverlayMode();
            }

            if (mode === 'infinite') {
                removeFocusTrapInfiniteMode();
            }
        }
	};
}

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

    // TODO: Maybe this can be refactored into being a watch thingy detecting when add/remove of an infinite overlay is done so we can skip the timeout function - But it works for now
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

// TODO: Consider adding a tablock method to avoid the possibility of escaping to the browser address bar - But maybe have a discussion about this in the PR instead?...

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

        // TODO: Figure out why it's necessary to make use of setTimeout and why it's necessary to remove the focus attributes
        setTimeout(function(){
            $(firstFocusableElement).removeAttr('auto-umb-focus').removeAttr('focus-on-filled').focus();
            $(firstFocusableElement).attr('auto-umb-focus');
        },100);
    }
}

angular.module('umbraco.services').factory('focusTrapService', focusTrapService);
