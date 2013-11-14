angular.module("umbraco.directives")
.directive('umbContextMenu', function ($injector, umbModelMapper, treeService) {
    return {
        scope: {
            menuDialogTitle: "@",
            currentSection: "@",
            currentNode: "=",
            menuActions: "="
        },
        restrict: 'E',
        replace: true,
        templateUrl: 'views/directives/umb-contextmenu.html',
        link: function (scope, element, attrs, ctrl) {
            
            //adds a handler to the context menu item click, we need to handle this differently
            //depending on what the menu item is supposed to do.
            scope.executeMenuItem = function (action) {

                if (action.metaData && action.metaData["jsAction"] && angular.isString(action.metaData["jsAction"])) {

                    //we'll try to get the jsAction from the injector
                    var menuAction = action.metaData["jsAction"].split('.');
                    if (menuAction.length !== 2) {

                        //if it is not two parts long then this most likely means that it's a legacy action                         
                        var js = action.metaData["jsAction"].replace("javascript:", "");
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
                            //map our content object to a basic entity to pass in to the menu handlers,
                            //this is required for consistency since a menu item needs to be decoupled from a tree node since the menu can
                            //exist standalone in the editor for which it can only pass in an entity (not tree node).
                            entity: umbModelMapper.convertToEntityBasic(scope.currentNode),
                            action: action,
                            section: scope.currentSection,
                            treeAlias: treeService.getTreeAlias(scope.currentNode)
                        }]);
                    }
                }
                else {
                    //by default we launch the dialog
                    
                    //TODO: This is temporary using $parent, now that this is an isolated scope
                    // the problem with all these dialogs is were passing other object's scopes around which isn't nice at all.
                    // Each of these passed scopes expects a .nav property assigned to it which is a reference to the navigationService,
                    // which should not be happenning... should simply be using the navigation service, no ?!
                    scope.$parent.openDialog(scope.currentNode, action, scope.currentSection);
                }
            };

        }
    };
});