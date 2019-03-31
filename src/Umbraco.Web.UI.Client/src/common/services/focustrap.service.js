/** Used to improve accesibility ensuring that the focus is trapped inside the active overlay and also ensuring aria-hidden is set accordingly so potential screen readers don't get confused about the context either */

function focusTrapService(eventsService) {

    // TODO: If possible I would like to store all the needed DOM references in an elements object, wich could then be passed to the methods so we don't need to reference and store them in each method
    // But some of the elements are not available untill the app.authenticated event happens... Not sure about the best practice around this...
    console.log('trap da focus mayn!');
    
    return {
        addFocusTrap: function(mode){

            if(mode === 'overlay'){
                addFocusTrapOverlayMode();
            }

            if(mode === 'infinite'){
                addFocusTrapInfiniteMode();
            }

            //TODO:
            // addFocusTrap
            // Check if there is at least 1 item (infinite or modal) and then add the trap

            // removeFocusTrap
            // If there are 0 items then by all means remove the focus trap

            // updateFocusTrap
            // If we're in infinite editing mode then whenever a there is more than 1 item we need to update the focus trap to target the correct infinite modal
            // Also ensuring that we don't touch the DOM for those fixed elements where the inert and aria-hidden attributes have already been added

            // Maybe add a "infinte editor check" and a "modal" check. Maybe just add a "type" param that needs to be either "modal" or "infinite editor" in order for calling the correct method to deal with the DOM manipulation?
        },
        removeFocusTrap: function (mode) {
            console.log('Remove that trap!');
        
            if (mode === 'overlay') {
                removeFocusTrapOverlayMode();
            }

            if (mode === 'infinite') {
                removeFocusTrapInfiniteMode();
            }
            //TODO: Simply find all inert and aria-hidden attributes and remove them?....
        }
	};
}

function addFocusTrapOverlayMode () {
    var mainWrapper = $('#mainwrapper');

    // TODO: Add inert and aria-hidden attributes to the mainWrapper and remove it again once the modal is removed

    console.log('ADD the focus trap for the OVERLAY mode, hehehehe');
}

function removeFocusTrapOverlayMode () {
    var mainWrapper = $('#mainwrapper');

    // TODO: Add inert and aria-hidden attributes to the mainWrapper and remove it again once the modal is removed

    console.log('REMOVE the focus trap for the OVERLAY mode, hehehehe');
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

    console.log(appHeader);
    console.log(leftColumn);
    console.log(contentColumn);

    // Remove the inert attribute from the key elements so they're tabable once the infinite editing mode has been deactivated
    appHeader.removeAttr('inert');
    leftColumn.removeAttr('inert');
    contentColumn.removeAttr('inert');

    console.log('inert removed and everything is back to normal....as you were!');
}

// TODO: Make sure to set focus on the first focusable element in the focusable overlay (There is a method for that somewhere :-))
// TODO: Consider adding a tablock method to avoid the possibility of escaping to the browser address bar - But maybe have a discussion about this in the PR instead?...

function toggleInert (editors) {
    const editorChildren = editors.children();
    const lastEditorChildIndex = editorChildren.length - 1;
    const lastEditorChild = $(editorChildren[lastEditorChildIndex]);

    editorChildren.attr('inert','');
    lastEditorChild.removeAttr('inert');
}

angular.module('umbraco.services').factory('focusTrapService', focusTrapService);
