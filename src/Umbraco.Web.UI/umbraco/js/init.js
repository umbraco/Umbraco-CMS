/** Executed when the application starts */
app.run(['userService', '$log', '$rootScope', '$location', 'navigationService', 'appState', 'editorState',
    function(userService, $log, $rootScope, $location, navigationService, appState, editorState) {

        var firstRun = true;

        /** when we have a successful first route that is not the login page - meaning the user is authenticated
            we'll get the current user from the user service and ensure it broadcasts it's events. If the route
            is successful from after a login then this will not actually do anything since the authenticated event would
            have alraedy fired, but if the user is loading the angularjs app for the first time and they are already authenticated
            then this is when the authenticated event will be fired.
        */
        $rootScope.$on('$routeChangeSuccess', function(event, current, previous) {
            if (firstRun && !$location.url().toLowerCase().startsWith("/login")) {
                firstRun = false;
                userService.getCurrentUser({ broadcastEvent: true });
            }

            if(current.params.section){
                $rootScope.locationTitle = current.params.section + " - " + $location.$$host;
            }
            else {
                $rootScope.locationTitle = "Umbraco - " + $location.$$host;
            }
            
            //reset the editorState on each successful route chage
            editorState.reset();

        });

        /** When the route change is rejected - based on checkAuth - we'll prevent the rejected route from executing including
            wiring up it's controller, etc... and then redirect to the rejected URL.   */
        $rootScope.$on('$routeChangeError', function(event, current, previous, rejection) {
            event.preventDefault();
            $location.path(rejection.path).search(rejection.search);
        });

        /* this will initialize the navigation service once the application has started */
        navigationService.init();

        //check for touch device, add to global appState
        var touchDevice = ("ontouchstart" in window || window.touch || window.navigator.msMaxTouchPoints === 5 || window.DocumentTouch && document instanceof DocumentTouch);
        appState.setGlobalState("touchDevice", touchDevice);
        
    }]);