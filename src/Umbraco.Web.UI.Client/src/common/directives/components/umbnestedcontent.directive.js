angular.module("umbraco.directives").directive('umbNestedContentEditor', [

    function () {

        var link = function ($scope, el, attrs) {

            // Clone the model because some property editors
            // do weird things like updating and config values
            // so we want to ensure we start from a fresh every
            // time, we'll just sync the value back when we need to
            $scope.model = Utilities.copy($scope.ngModel);
            $scope.nodeContext = $scope.model;

            $scope.readonly = false;

            attrs.$observe('readonly', (value) => {
                $scope.readonly = value !== undefined;
            });

            // Find the selected tab
            var selectedTab = $scope.model.variants[0].tabs[0];

            if ($scope.tabAlias) {
                Utilities.forEach($scope.model.variants[0].tabs, tab => {
                    if (tab.alias.toLowerCase() === $scope.tabAlias.toLowerCase()) {
                        selectedTab = tab;
                        return;
                    }
                });
            }

            $scope.tab = selectedTab;

            // Listen for sync request
            var unsubscribe = $scope.$on("ncSyncVal", function (ev, args) {
                if (args.key === $scope.model.key) {

                    // Tell inner controls we are submitting
                    $scope.$broadcast("formSubmitting", { scope: $scope });

                    // Sync the values back
                    Utilities.forEach($scope.ngModel.variants[0].tabs, tab => {
                        if (tab.alias.toLowerCase() === selectedTab.alias.toLowerCase()) {

                            var localPropsMap = selectedTab.properties.reduce((map, obj) => {
                                map[obj.alias] = obj;
                                return map;
                            }, {});

                            Utilities.forEach(tab.properties, prop => {
                                if (localPropsMap.hasOwnProperty(prop.alias)) {
                                    prop.value = localPropsMap[prop.alias].value;
                                }
                            });
                        }
                    });
                    
                }
            });

            $scope.$on('$destroy', function () {
                unsubscribe();
            });
        };

        return {
            restrict: "E",
            replace: true,
            templateUrl: Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + "/views/propertyeditors/nestedcontent/nestedcontent.editor.html",
            scope: {
                ngModel: '=',
                tabAlias: '='
            },
            link: link
        };

    }
]);

//angular.module("umbraco.directives").directive('nestedContentSubmitWatcher', function () {
//    var link = function (scope) {
//        // call the load callback on scope to obtain the ID of this submit watcher
//        var id = scope.loadCallback();
//        scope.$on("formSubmitting", function (ev, args) {
//            // on the "formSubmitting" event, call the submit callback on scope to notify the nestedContent controller to do it's magic
//            if (id === scope.activeSubmitWatcher) {
//                scope.submitCallback();
//            }
//        });
//    }

//    return {
//        restrict: "E",
//        replace: true,
//        template: "",
//        scope: {
//            loadCallback: '=',
//            submitCallback: '=',
//            activeSubmitWatcher: '='
//        },
//        link: link
//    }
//});
