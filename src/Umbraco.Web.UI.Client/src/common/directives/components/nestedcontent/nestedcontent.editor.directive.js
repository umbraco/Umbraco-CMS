angular.module("umbraco.directives").directive('nestedContentEditor', [

    function () {

        var link = function($scope) {
            $scope.nodeContext = $scope.model = $scope.ngModel;

            var tab = $scope.ngModel.tabs[0];

            if ($scope.tabAlias) {
                angular.forEach($scope.ngModel.tabs, function(value, key) {
                    if (value.alias.toLowerCase() === $scope.tabAlias.toLowerCase()) {
                        tab = value;
                        return;
                    }
                });
            }

            $scope.tab = tab;

            var unsubscribe = $scope.$on("ncSyncVal", function(ev, args) {
                if (args.key === $scope.model.key) {
                    $scope.$broadcast("formSubmitting", { scope: $scope });
                }
            });

            $scope.$on('$destroy', function() {
                unsubscribe();
            });
        };

        return {
            restrict: "E",
            replace: true,
            template: "<div class=\"umb-pane\"><umb-property property=\"property\" ng-repeat=\"property in tab.properties\"><umb-editor model=\"property\"></umb-editor></umb-property></div>",
            scope: {
                ngModel: '=',
                tabAlias: '='
            },
            link: link
        };

    }
]);