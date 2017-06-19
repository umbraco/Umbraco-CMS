angular.module("umbraco").controller("Umbraco.Dashboards.Help.CoreHelpController", function ($scope, $routeParams, $timeout, eventsService, userService, helpService, localizationService) {

    $scope.section = $routeParams.section;
    $scope.tree = $routeParams.tree;

    var stateKey = $scope.section + $scope.tree;

    if (!$scope.section) {
        $scope.section = "content";
    }

    userService.getCurrentUser().then(function (user) {

        $scope.usertype = user.userType;
        $scope.lang = user.locale;

        evts.push(eventsService.on("appState.treeState.changed", function (e, args) {

            $timeout(function () {
                if ($scope.section !== $routeParams.section || $scope.tree !== $routeParams.tree) {
                    $scope.section = $routeParams.section;
                    $scope.tree = $routeParams.tree;

                    findHelp($scope.section, $scope.tree, $scope.usertype, $scope.lang);
                }
            }, 500);
            
        }));

        findHelp($scope.section, $scope.tree, $scope.usertype, $scope.lang);

    });

    var findHelp = function (section, tree, usertype, lang) {

        helpService.getContextHelpForPage(section, tree).then(function (topics) {
            $scope.topics = topics;
        });

    };

    var evts = [];

   
    $scope.$on('$destroy', function () {
        for (var e in evts) {
            eventsService.unsubscribe(evts[e]);
        }
    });




});