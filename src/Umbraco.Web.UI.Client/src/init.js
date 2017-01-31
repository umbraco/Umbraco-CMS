/** Executed when the application starts, binds to events and set global state */
app.run(['userService', '$log', '$rootScope', '$location', 'navigationService', 'appState', 'editorState', 'fileManager', 'assetsService', 'eventsService', '$cookies', '$templateCache', 'localStorageService',
    function (userService, $log, $rootScope, $location, navigationService, appState, editorState, fileManager, assetsService, eventsService, $cookies, $templateCache, localStorageService) {

        //This sets the default jquery ajax headers to include our csrf token, we
        // need to user the beforeSend method because our token changes per user/login so
        // it cannot be static
        $.ajaxSetup({
            beforeSend: function (xhr) {
                xhr.setRequestHeader("X-XSRF-TOKEN", $cookies["XSRF-TOKEN"]);
            }
        });

        /** Listens for authentication and checks if our required assets are loaded, if/once they are we'll broadcast a ready event */
        eventsService.on("app.authenticated", function(evt, data) {
            
            assetsService._loadInitAssets().then(function() {
                appState.setGlobalState("isReady", true);

                //send the ready event with the included returnToPath,returnToSearch data
                eventsService.emit("app.ready", data);
                returnToPath = null, returnToSearch = null;
            });
        });

        /** execute code on each successful route */
        $rootScope.$on('$routeChangeSuccess', function(event, current, previous) {

            if(current.params.section){
                $rootScope.locationTitle = current.params.section + " - " + $location.$$host;
            }
            else {
                $rootScope.locationTitle = "Umbraco - " + $location.$$host;
            }

            //reset the editorState on each successful route chage
            editorState.reset();

            //reset the file manager on each route change, the file collection is only relavent
            // when working in an editor and submitting data to the server.
            //This ensures that memory remains clear of any files and that the editors don't have to manually clear the files.
            fileManager.clearFiles();
        });

        /** When the route change is rejected - based on checkAuth - we'll prevent the rejected route from executing including
            wiring up it's controller, etc... and then redirect to the rejected URL.   */
        $rootScope.$on('$routeChangeError', function(event, current, previous, rejection) {
            event.preventDefault();

            var returnPath = null;
            if (rejection.path == "/login" || rejection.path.startsWith("/login/")) {
                //Set the current path before redirecting so we know where to redirect back to
                returnPath = encodeURIComponent($location.url());
            }

            $location.path(rejection.path)
            if (returnPath) {
                $location.search("returnPath", returnPath);
            }

        });


        /* this will initialize the navigation service once the application has started */
        navigationService.init();

        //check for touch device, add to global appState
        //var touchDevice = ("ontouchstart" in window || window.touch || window.navigator.msMaxTouchPoints === 5 || window.DocumentTouch && document instanceof DocumentTouch);
        var touchDevice =  /android|webos|iphone|ipad|ipod|blackberry|iemobile|touch/i.test(navigator.userAgent.toLowerCase());
        appState.setGlobalState("touchDevice", touchDevice);
    }]);
