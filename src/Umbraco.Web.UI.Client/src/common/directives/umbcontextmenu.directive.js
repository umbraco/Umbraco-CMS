angular.module("umbraco.directives")
.directive('umbContextMenu', function ($injector) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: 'views/directives/umb-contextmenu.html',
        link: function (scope, element, attrs, ctrl) {

            //adds a handler to the context menu item click, we need to handle this differently
            //depending on what the menu item is supposed to do.
            scope.executeMenuItem = function (currentNode, action, currentSection) {

                if (action.metaData && action.metaData["jsAction"] && angular.isString(action.metaData["jsAction"])) {

                    //we'll try to get the jsAction from the injector
                    var menuAction = action.metaData["jsAction"].split('.');
                    if (menuAction.length !== 2) {

                        //if it is not two parts long then this most likely means that it's a legacy action                         
                        var js = action.metaData["jsAction"];
                        //there's not really a different way to acheive this except for eval 
                        eval(js);

                    }
                    else {
                        var service = $injector.get(menuAction[0]);
                        if (!service) {
                            throw "The angular service " + menuAction[0] + " could not be found";
                        }

                        var method = service[menuAction[1]];

                        if (!method) {
                            throw "The method " + menuAction[1] + " on the angular service " + menuAction[0] + " could not be found";
                        }

                        method.apply(this, [{
                            treeNode: currentNode,
                            action: action,
                            section: currentSection
                        }]);
                    }
                }
                else {
                    //by default we launch the dialog
                    scope.openDialog(currentNode, action, currentSection);
                }
            };

        }
    };
});