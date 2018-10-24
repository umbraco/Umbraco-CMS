angular.module("umbraco.directives")
    .directive('disableTabindex', function () {

    return {
        restrict: 'A', //Can only be used as an attribute
        scope: { disableTabindex: "<"},
        link: function (scope, element, attrs) {

            function enableTest(){
                //Add in observer code

                //Use DOM Mutation Observer
                //Select the node that will be observed for mutations (native DOM element not jQLite version)
                var targetNode = element[0];

                // Options for the observer (which mutations to observe)
                var config = { attributes: false, childList: true, subtree: false };

                // Callback function to execute when mutations are observed
                var callback = function(mutationsList, observer) {
                    for(var mutation of mutationsList) {

                        console.log('mutation', mutation);

                        //DOM items have been added or removed
                        if (mutation.type == 'childList') {

                            //Check if any child items in mutation.target contain an input
                            var jqLiteEl = angular.element(mutation.target);
                            var childInputs = jqLiteEl.find('input');

                            //For each item in childInputs - override or set HTML attribute tabindex="-1"
                            angular.forEach(childInputs, function(element){
                                console.log('item in loop', element);

                                //TODO: Get existing element & it's tabindex (if any set)


                                //TODO: Note we updating way too many times from the DOMSubtreeModified event - is this expensive?
                                angular.element(element).attr('tabindex', '-1');
                            });
                        }
                    }
                };

                // Create an observer instance linked to the callback function
                var observer = new MutationObserver(callback);

                // Start observing the target node for configured mutations
                //GO GO GO
                observer.observe(targetNode, config);
            };


            scope.$watch('disableTabindex',(newVal, oldVal) =>{
                console.log('new val', newVal);

                if(newVal === true){
                    enableTest();
                }else{

                }
            });






            //TODO: Unsure if we need to do this - to ensure the browser not trying to notify us still
            //When we browse away from the page
            // element.on('$destroy', function(e){
            //     console.log('element with disable-tabindex attribute is destoryed');

            //     //Remove/stop the observer
            //     observer.disconnect();
            // });

        }
    };
});