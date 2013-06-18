//Handles the section area of the app
angular.module('umbraco').controller("NavigationController",
    function ($scope, navigationService) {
    
    //load navigation service handlers
    $scope.changeSection = navigationService.changeSection;    
    $scope.showTree = navigationService.showTree;
    $scope.hideTree = navigationService.hideTree;
    $scope.hideMenu = navigationService.hideMenu;
    $scope.showMenu = navigationService.showMenu;
    $scope.hideDialog = navigationService.hideDialog;
    $scope.hideNavigation = navigationService.hideNavigation;
    $scope.ui = navigationService.ui;    

    $scope.selectedId = navigationService.currentId;
    $scope.sections = navigationService.sections();
    
    //events
    $scope.$on("treeOptionsClick", function(ev, args){
            $scope.currentNode = args.node;
            args.scope = $scope;
            navigationService.showMenu(ev, args);
    });

    $scope.openDialog = function(currentNode,action,currentSection){
        navigationService.showDialog({
                                        scope: $scope,
                                        node: currentNode,
                                        action: action,
                                        section: currentSection});
    };
});


angular.module('umbraco').controller("SearchController", function ($scope, searchService, $log, navigationService) {

    var currentTerm = "";
    $scope.deActivateSearch = function(){
       currentTerm = ""; 
    };

    $scope.performSearch = function (term) {
        if(term != undefined && term != currentTerm){
            if(term.length > 3){
                $scope.ui.selectedSearchResult = -1;
                navigationService.showSearch();
                currentTerm = term;
                $scope.ui.searchResults = searchService.search(term, $scope.currentSection);
            }else{
                $scope.ui.searchResults = [];
            }
        }
    };    

    $scope.hideSearch = navigationService.hideSearch;

    $scope.iterateResults = function (direction) {
       if(direction == "up" && $scope.ui.selectedSearchResult < $scope.ui.searchResults.length) 
            $scope.ui.selectedSearchResult++;
        else if($scope.ui.selectedSearchResult > 0)
            $scope.ui.selectedSearchResult--;
    };

    $scope.selectResult = function () {
        navigationService.showMenu($scope.ui.searchResults[$scope.ui.selectedSearchResult], undefined);
    };
});


angular.module('umbraco').controller("DashboardController", function ($scope, $routeParams, scriptLoader) {
    $scope.name = $routeParams.section;

    scriptLoader.load(['http://www.google.com/jsapi'])
        .then(function(){
            google.load("maps", "3",
                        {
                            callback: function () {
                                
                                //Google maps is available and all components are ready to use.
                                var mapOptions = {
                                   zoom: 8,
                                   center: new google.maps.LatLng(-34.397, 150.644),
                                   mapTypeId: google.maps.MapTypeId.ROADMAP
                                 };

                                var mapDiv = document.getElementById('test_map');
                                var map = new google.maps.Map(mapDiv, mapOptions);

                            },
                            other_params: "sensor=false"
                        });
    });

});


//handles authentication and other application.wide services
angular.module('umbraco').controller("MainController", 
    function ($scope, $routeParams, $rootScope, notificationsService, userService, navigationService) {
    
    //also be authed for e2e test
    var d = new Date();
    var weekday = new Array("Super Sunday", "Manic Monday", "Tremendous Tuesday", "Wonderfull Wednesday", "Thunder Thursday", "Friendly Friday", "Shiny Saturday");
    $scope.today = weekday[d.getDay()];
    

    $scope.signin = function () {
        $scope.authenticated = userService.authenticate($scope.login, $scope.password);

        if($scope.authenticated){
            $scope.user = userService.getCurrentUser();
        }
    };

    $scope.signout = function () {
        userService.signout();
        $scope.authenticated = false;
    };
    

    //subscribes to notifications in the notification service
    $scope.notifications = notificationsService.current;
    $scope.$watch('notificationsService.current', function (newVal, oldVal, scope) {
        if (newVal) {
            $scope.notifications = newVal;
        }
    });

    $scope.removeNotification = function(index) {
        notificationsService.remove(index);
    };

    $scope.closeDialogs = function(event){

        $rootScope.$emit("closeDialogs");

        if(navigationService.ui.stickyNavigation && $(event.target).parents(".umb-modalcolumn").size() == 0){ 
            navigationService.hideNavigation();
        }
    };

    if (userService.authenticated) {
        $scope.signin();
    }
});
