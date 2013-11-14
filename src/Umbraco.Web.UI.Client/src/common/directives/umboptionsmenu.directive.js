angular.module("umbraco.directives")
.directive('umbOptionsMenu', function ($injector, treeService, navigationService, umbModelMapper) {
    return {
        scope: {
            content: "=",
            currentSection: "@",
            treeAlias: "@"
        },
        restrict: 'E',
        replace: true,
        templateUrl: 'views/directives/umb-optionsmenu.html',
        link: function (scope, element, attrs, ctrl) {

            //adds a handler to the context menu item click, we need to handle this differently
            //depending on what the menu item is supposed to do.
            scope.executeMenuItem = function (action) {

                //map our content object to a basic entity to pass in to the handlers
                var currentEntity = umbModelMapper.convertToEntityBasic(scope.content);

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
                            entity: currentEntity,
                            action: action,
                            section: scope.currentSection,
                            treeAlias: scope.treeAlias
                        }]);
                    }
                }
                else {
                    //by default we launch the dialog

                    //TODO: This is temporary using $parent, now that this is an isolated scope
                    // the problem with all these dialogs is were passing other object's scopes around which isn't nice at all.
                    // Each of these passed scopes expects a .nav property assigned to it which is a reference to the navigationService,
                    // which should not be happenning... should simply be using the navigation service, no ?!
                    scope.$parent.openDialog(currentEntity, action, scope.currentSection);
                }
            };

            //callback method to go and get the options async
            scope.getOptions = function () {
                if (!scope.content.id) {
                    return;
                }

                if (!scope.actions) {
                    treeService.getMenu({ treeNode: navigationService.ui.currentNode })
                        .then(function (data) {
                            scope.actions = data.menuItems;
                        });
                }
            };

        }
    };
});