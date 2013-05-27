'use strict';
//From:     

angular.module("stateManager", [])
	.factory("stateManager", ["$rootScope", "$location", function factory(rootScope, location) {
	    //define the StateManager object
	    function StateManager() {
	        var initialiserStack = new Array();

	        //register handler to detect when URL changes. try to use onpopstate, and fallback to onhashchange
	        if (history && history.pushState) {
	            window.onpopstate = function (event) {
	                respondToStateChange(event);
	            }
	        }
	        else {
	            window.onhashchange = function (event) {
	                respondToStateChange(event);
	            }
	        }

	        this.registerInitialiser = function (initialiser) {
	            //initialise the controller by calling its constructor, because the controller was just loaded
	            initialiser(getProperPathComponents());

	            //add it to the stack so it can be reinitialised in case the state changes in the future due to onhashchange
	            initialiserStack.push(initialiser);

	            //return a function to the controller that it should execute immediately; it will make that controller report back to the statemanager when its destroyed so that we can free up memory
	            return function (ctrlScope) {
	                ctrlScope.$on("$destroy", function () {
	                    var index = initialiserStack.indexOf(initialiser);
	                    if (index != -1) {
	                        initialiserStack.splice(index, 1);
	                    }
	                })
	            }
	        }

	        this.replaceState = function (modifiedPathComponents) {
	            var newPath = "/" + mergePathComponents(modifiedPathComponents).join("/");
	            location.path(newPath).replace();
	        }

	        this.pushState = function (modifiedPathComponents) {
	            var newPath = "/" + mergePathComponents(modifiedPathComponents).join("/");
	            location.path(newPath);
	        }

	        /* utils */
	        function respondToStateChange(event) {
	            //freeze stack
	            var frozenStack = new Array();
	            for (var i in initialiserStack) {
	                frozenStack[i] = initialiserStack[i];
	            }

	            //initialise the stack discrepancy as the length of the array, we should stop there anyway and we need a way to detect when it trickles down
	            var stackDiscrepancyIndex = initialiserStack.length;

	            //run each initialiser, but break out when the stack changes
	            for (var i in initialiserStack) {
	                //detect if the initialiser that is about the be executed has been replaced, in which case we can assume the controller stack from this point on is fresh and has already been initialised so we don't need to continue
	                var newStackDiscrepancyIndex = detectStackDiscrepancy(frozenStack, initialiserStack);
	                if (newStackDiscrepancyIndex != -1) {//-1 signifies no discrepancy
	                    var stackDiscrepancyIndex = newStackDiscrepancyIndex < stackDiscrepancyIndex ? newStackDiscrepancyIndex : stackDiscrepancyIndex;
	                    if (i >= stackDiscrepancyIndex) {
	                        break;
	                    }
	                }

	                //run initialiser
	                initialiserStack[i](getProperPathComponents());
	                rootScope.$apply();
	            }
	        }

	        function detectStackDiscrepancy(stackA, stackB) {
	            var stackDiscrepancyIndex = -1;
	            for (var i in stackB) {
	                if (stackA[i] != stackB[i]) {
	                    stackDiscrepancyIndex = i;
	                    break;
	                }
	            }

	            return stackDiscrepancyIndex;
	        }

	        function mergePathComponents(modifiedPathComponents) {
	            //merge the changes into a new array
	            var currentPathComponents = getProperPathComponents();
	            var newPathComponents = new Array();
	            for (var i in modifiedPathComponents) {
	                if (modifiedPathComponents[i]) {
	                    newPathComponents[i] = modifiedPathComponents[i];
	                } else {
	                    newPathComponents[i] = currentPathComponents[i];
	                }
	            }

	            return newPathComponents;
	        }

	        function getProperPathComponents() {
	            return location.path().substring(1).split("/");
	        }
	    }

	    //return a new instance of the StateManager
	    return new StateManager();
	}])