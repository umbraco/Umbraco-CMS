/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTabs 
* @restrict E
* @function
* @description 
* Used to control tab changes in editors, this directive must be placed around any editors' tabs (including header tabs and body tabs)
**/
angular.module("umbraco.directives")
    .directive('umbTabs', function() {
        return {                        
            //uses a controller to expose an API
            controller: function ($scope, $attrs, $element, $parse) {

                //NOTE: This whole $parse strategy is the same type of thing used by ngModel, we need to use it
                // because angular 1.1.5 is stupid. Normally we could just make a scope with =umbTags but that doesn't
                // work for attribute type directives, seems to only work for element directives and I don't want this to 
                // be an element directive.... so we're using this.
                var tabsValueGet = $parse($attrs.umbTabs);

                //internal props

                var activeTabId = null;
                var tabCollectionChangedCallbacks = [];
                var activeTabChangedCallbacks = [];
                var firstRun = false;
                var tabs = [];

                //public props/methods

                this.getActiveTab = function() {
                    return activeTabId;
                };
                this.setActiveTab = function(tabId) {
                    activeTabId = tabId;
                    for (var callback in activeTabChangedCallbacks) {
                        activeTabChangedCallbacks[callback](activeTabId);
                    }
                };
                this.onTabCollectionChanged = function (callback) {
                    tabCollectionChangedCallbacks.push(callback);
                };
                this.onActiveTabChanged = function (callback) {
                    activeTabChangedCallbacks.push(callback);
                };

                $scope.$watch(function() {
                    return tabsValueGet($scope);
                }, function (newValue, oldValue) {
                    if ((angular.isArray(newValue) && angular.isArray(oldValue) && newValue.length !== oldValue.length|| (newValue !== undefined && oldValue === undefined))) {

                        tabs = []; //reset first
                        for (var val in newValue) {
                            var tab = { id: newValue[val].id, label: newValue[val].label };
                            tabs.push(tab);
                        }

                        //set active tab to the first one - one time
                        if (firstRun === false) {
                            firstRun = true;
                            if (tabs.length > 0) {
                                activeTabId = tabs[0].id;
                                for (var activeTabCallback in activeTabChangedCallbacks) {
                                    activeTabChangedCallbacks[activeTabCallback](activeTabId);
                                }
                            }
                        }
                        
                        for (var callback in tabCollectionChangedCallbacks) {
                            tabCollectionChangedCallbacks[callback](tabs);
                        }
                    }
                });
                
                $scope.$on('$destroy', function () {
                    tabCollectionChangedCallbacks = [];
                    activeTabChangedCallbacks = [];
                });

            }
        };
    });