
/**
 * @ngdoc controller
 * @name Umbraco.MainController
 * @function
 * 
 * @description
 * The main application controller
 * 
 */
function MainController($scope, $rootScope, $location, $routeParams, $timeout, $http, $log, appState, treeService, notificationsService, userService, navigationService, historyService, updateChecker, assetsService, eventsService, umbRequestHelper, tmhDynamicLocale) {

    //the null is important because we do an explicit bool check on this in the view
    //the avatar is by default the umbraco logo    
    $scope.authenticated = null;
    $scope.avatar = "assets/img/application/logo.png";
    $scope.touchDevice = appState.getGlobalState("touchDevice");
    

    $scope.removeNotification = function (index) {
        notificationsService.remove(index);
    };

    $scope.closeDialogs = function (event) {
        //only close dialogs if non-link and non-buttons are clicked
        var el = event.target.nodeName;
        var els = ["INPUT","A","BUTTON"];

        if(els.indexOf(el) >= 0){return;}

        var parents = $(event.target).parents("a,button");
        if(parents.length > 0){
            return;
        }

        //SD: I've updated this so that we don't close the dialog when clicking inside of the dialog
        var nav = $(event.target).parents("#dialog");
        if (nav.length === 1) {
            return;
        }

        eventsService.emit("app.closeDialogs", event);
    };

    var evts = [];

    //when a user logs out or timesout
    evts.push(eventsService.on("app.notAuthenticated", function() {
        $scope.authenticated = null;
        $scope.user = null;
    }));
    
    //when the app is read/user is logged in, setup the data
    evts.push(eventsService.on("app.ready", function (evt, data) {
        
        $scope.authenticated = data.authenticated;
        $scope.user = data.user;

        updateChecker.check().then(function(update){
            if(update && update !== "null"){
                if(update.type !== "None"){
                    var notification = {
                           headline: "Update available",
                           message: "Click to download",
                           sticky: true,
                           type: "info",
                           url: update.url
                    };
                    notificationsService.add(notification);
                }
            }
        })

        //if the user has changed we need to redirect to the root so they don't try to continue editing the
        //last item in the URL (NOTE: the user id can equal zero, so we cannot just do !data.lastUserId since that will resolve to true)
        if (data.lastUserId !== undefined && data.lastUserId !== null && data.lastUserId !== data.user.id) {
            $location.path("/").search("");
            historyService.removeAll();
            treeService.clearCache();
        }

        //Load locale file
        if ($scope.user.locale) {
            tmhDynamicLocale.set($scope.user.locale);
        }

        if($scope.user.emailHash){
            $timeout(function () {                
                //yes this is wrong.. 
                $("#avatar-img").fadeTo(1000, 0, function () {
                    $timeout(function () {
                        //this can be null if they time out
                        if ($scope.user && $scope.user.emailHash) {
                            $scope.avatar = "//www.gravatar.com/avatar/" + $scope.user.emailHash + ".jpg?s=64&d=mm";
                        }
                    });
                    $("#avatar-img").fadeTo(1000, 1);
                });
                
              }, 3000);  
        }

    }));

    //ensure to unregister from all events!
    $scope.$on('$destroy', function () {
        for (var e in evts) {
            eventsService.unsubscribe(evts[e]);
        }
    });

}


//register it
angular.module('umbraco').controller("Umbraco.MainController", MainController).
        config(function (tmhDynamicLocaleProvider) {
            //Set url for locale files
            tmhDynamicLocaleProvider.localeLocationPattern('lib/angular/1.1.5/i18n/angular-locale_{{locale}}.js');
        });