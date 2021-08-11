angular.module("umbraco.directives")
    .directive('disableTabindex', function (tabbableService) {

        return {
            restrict: 'A', //Can only be used as an attribute,
            scope: {
                "disableTabindex": "<"
            },
            link: function (scope, element, attrs) {

                if (scope.disableTabindex) {
                    //Select the node that will be observed for mutations (native DOM element not jQLite version)
                    var targetNode = element[0];

                    //Watch for DOM changes - so when the property editor subview loads in
                    //We can be notified its updated the child elements inside the DIV we are watching
                    var observer = new MutationObserver(domChange);

                    // Options for the observer (which mutations to observe)
                    var config = { attributes: true, childList: true, subtree: true };

                    function domChange(mutationsList, observer) {
                        for (var mutation of mutationsList) {

                            //DOM items have been added or removed
                            if (mutation.type == 'childList') {

                                //Check if any child items in mutation.target contain an input
                                var childInputs = tabbableService.tabbable(mutation.target);

                                //For each item in childInputs - override or set HTML attribute tabindex="-1"
                                Utilities.forEach(childInputs, element => {
                                    $(element).attr('tabindex', '-1');
                                });
                            }
                        }
                    }

                    // Start observing the target node for configured mutations
                    //GO GO GO
                    observer.observe(targetNode, config);
                }

            }
        };
    });
