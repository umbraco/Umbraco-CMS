/**
* @ngdoc directive
* @name umbraco.directives.directive:umbSections
* @restrict E
**/
function sectionsDirective($timeout, $window, navigationService, treeService, sectionResource, appState, eventsService, $location) {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/directives/umb-sections.html',
        link: function (scope, element, attr, ctrl) {
			
            //setup scope vars
			scope.maxSections = 7;
			scope.overflowingSections = 0;
            scope.sections = [];
            scope.currentSection = appState.getSectionState("currentSection");
            scope.showTray = false; //appState.getGlobalState("showTray");
            scope.stickyNavigation = appState.getGlobalState("stickyNavigation");
            scope.needTray = false;
            scope.trayAnimation = function() {                
                if (scope.showTray) {
                    return 'slide';
                }
                else if (scope.showTray === false) {
                    return 'slide';
                }
                else {
                    return '';
                }
            };

			function loadSections(){
				sectionResource.getSections()
					.then(function (result) {
						scope.sections = result;
						calculateHeight();
					});
			}

			function calculateHeight(){
				$timeout(function(){
					//total height minus room for avatar and help icon
					var height = $(window).height()-200;
					scope.totalSections = scope.sections.length;
					scope.maxSections = Math.floor(height / 70);
					scope.needTray = false;

					if(scope.totalSections > scope.maxSections){
						scope.needTray = true;
						scope.overflowingSections = scope.maxSections - scope.totalSections;
					}
				});
			}

			var evts = [];

            //Listen for global state changes
            evts.push(eventsService.on("appState.globalState.changed", function(e, args) {
                if (args.key === "showTray") {
                    scope.showTray = args.value;
                }
                if (args.key === "stickyNavigation") {
                    scope.stickyNavigation = args.value;
                }
            }));

            evts.push(eventsService.on("appState.sectionState.changed", function(e, args) {
                if (args.key === "currentSection") {
                    scope.currentSection = args.value;
                }
            }));

            evts.push(eventsService.on("app.reInitialize", function(e, args) {
                //re-load the sections if we're re-initializing (i.e. package installed)
                loadSections();
            }));

            //ensure to unregister from all events!
			scope.$on('$destroy', function () {
			    for (var e in evts) {
			        eventsService.unsubscribe(evts[e]);
			    }
			});

			//on page resize
			window.onresize = calculateHeight;
			
			scope.avatarClick = function(){
				navigationService.showUserDialog();
			};

			scope.helpClick = function(){
				navigationService.showHelpDialog();
			};

			scope.sectionClick = function (section) {
			    if (navigationService.userDialog) {
			        navigationService.userDialog.close();
			    }
			    if (navigationService.helpDialog) {
			        navigationService.helpDialog.close();
			    }
			    
			    navigationService.hideSearch();
			    navigationService.showTree(section.alias);
			    $location.path("/" + section.alias);
			};

			scope.sectionDblClick = function(section){
				navigationService.reloadSection(section.alias);
			};

			scope.trayClick = function () {
                // close dialogs
			    if (navigationService.userDialog) {
			        navigationService.userDialog.close();
			    }
			    if (navigationService.helpDialog) {
			        navigationService.helpDialog.close();
			    }

			    if (appState.getGlobalState("showTray") === true) {
			        navigationService.hideTray();
			    } else {
			        navigationService.showTray();
			    }
			};
            
			loadSections();

        }
    };
}

angular.module('umbraco.directives').directive("umbSections", sectionsDirective);
