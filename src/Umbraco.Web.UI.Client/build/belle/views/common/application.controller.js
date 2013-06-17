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


angular.module('umbraco').controller("DashboardController", function ($scope, $routeParams) {
    $scope.name = $routeParams.section;
});


//handles authentication and other application.wide services
angular.module('umbraco').controller("MainController", 
    function ($scope, $routeParams, $rootScope, $timeout, notificationsService, userService, navigationService) {
    
    //also be authed for e2e test
    var d = new Date();
    var weekday = new Array("Super Sunday", "Manic Monday", "Tremendous Tuesday", "Wonderfull Wednesday", "Thunder Thursday", "Friendly Friday", "Shiny Saturday");
    $scope.today = weekday[d.getDay()];


    //set default properties
    $scope.authenticated = null; //the null is important because we do an explicit bool check on this in the view    
    $scope.login = "";
    $scope.password = "";

    $scope.signin = function () {

        userService.authenticate($scope.login, $scope.password)
            .then(function(data) {
                $scope.authenticated = data.authenticated;
                $scope.user = data.user;
            }, function(reason) {
                alert(reason);
            });
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
        
    //fetch the authorized status         
    userService.isAuthenticated()
            .then(function (data) {
                $scope.authenticated = data.authenticated;
                $scope.user = data.user;
            }, function (reason) {
                alert("An error occurred checking authentication.");
                $scope.authenticated = false;
                $scope.user = null;
            });
});
