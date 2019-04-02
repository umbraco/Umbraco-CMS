(function () {
    'use strict';

    function umbInertAttributeDirective(eventsService) {
        var directive = {
            restrict: "A", // Can only be used as an attribute
            scope: {"umbInertAttribute":"@"},
            link: function (scope, element, attrs) {
                // If the value passed to the "umbInertAttribute" is "overlay" we will add/remove the inert attribute depending on what is emitted
                if (scope.umbInertAttribute === 'infinite-overlay') {
                    eventsService.on('appState.editors.open', function (name, args) {
                        console.log('an infinite editor just opened');

                        // The umb-editor needs a special touch :)
                        if (element.hasClass('umb-editor--infinityMode')) {
                            console.log(element,'what element are we dealing with?');
                            console.log(args,'what are the passed args?');
                        }
                        // Set the inert attribute if it's missing on the element
                        else {
                            if (!element.attr('inert')) {
                                element.attr('inert','');
                            }
                        }

                        // TODO: Perhaps a mutation observer is needed for the "umb-editor" stuff...
                    });
    
                    eventsService.on('appState.editors.close', function (name, args) {
                        console.log('an infinite editor just closed');
                        console.log(element,'what element are we dealing with?');
                        console.log(args,'what are the passed args?');

                        if (element.hasClass('umb-editor--infinityMode')) {
                            console.log('do something special');
                        }
                        // Figure out if any editors are still en view before removing the inert attribute
                        else {
                            if (args.editors.length === 0) {
                                element.removeAttr('inert');
                            }
                        }

                        // element.removeAttr('inert',''); // TODO: do something special if it's the "editors" element we're on AND make sure to check the args and see if the inert attribute needs to be removed
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

        return directive;

    }

    angular.module('umbraco.directives').directive('umbInertAttribute', umbInertAttributeDirective);

})();
