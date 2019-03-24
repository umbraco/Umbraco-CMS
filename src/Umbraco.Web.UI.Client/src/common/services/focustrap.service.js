/** Used to improve accesibility ensuring that the focus is trapped inside the active overlay and also ensuring aria-hidden is set accordingly so potential screen readers don't get confused about the context either */

function focusTrapService() {

    console.log('trap da focus mayn!');
    
    return {
        addFocusTrap: function(mode){

            if(mode === 'overlay'){
                // Call overlay helper method
                addFocusTrapOverlayMode();
            }

            if(mode === 'infinite'){
                // Call infinite helper method
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
        removeFocusTrap: function () {
            console.log('Remove that trap!');
        
            //TODO: Simply find all inert and aria-hidden attributes and remove them?....
        }
	};
}

function addFocusTrapOverlayMode () {
    var mainWrapper = $('#mainwrapper');

    // TODO: Add inert and aria-hidden attributes to the mainWrapper and remove it again once the modal is removed

    console.log('add the focus trap for the OVERLAY mode, hehehehe');
}

function addFocusTrapInfiniteMode () {
    var appHeader = $('.umb-app-header');
    var leftColumn = $('#leftcolumn');
    var contentColumn = $('#contentcolumn > div:first-child');

    appHeader.attr('inert','');
    leftColumn.attr('inert','');
    contentColumn.attr('inert','');
    var editors = $('.umb-editors'); // Keep an eye on the added / removed editors... not sure if watch is needed? But always add inert/aria-hidden to the prev siblings...

    console.log('add the focus trap for the INFINITE mode, hehehehe');
}

angular.module('umbraco.services').factory('focusTrapService', focusTrapService);
