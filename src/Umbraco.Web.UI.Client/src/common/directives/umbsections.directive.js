/**
* @ngdoc directive
* @name umbraco.directives.directive:umbSections
* @restrict E
**/
function sectionsDirective($timeout, $window, navigationService, treeService, sectionResource, appState) {
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
            scope.showTray = appState.getGlobalState("showTray");
            scope.stickyNavigation = appState.getGlobalState("stickyNavigation");
            scope.needTray = false;

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

            //Listen for global state changes
			scope.$on("appState.globalState.changed", function (e, args) {
			    if (args.key === "showTray") {
			        scope.showTray = args.value;
			    }
			    if (args.key === "stickyNavigation") {
			        scope.stickyNavigation = args.value;
			    }
			});

			//When the user logs in
			scope.$on("authenticated", function (evt, data) {
				//populate their sections if the user has changed
				if (data.lastUserId !== data.user.id) {
					loadSections();
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
			    navigationService.hideSearch();
				navigationService.showTree(section.alias);
			};

			scope.sectionDblClick = function(section){
				navigationService.reloadSection(section.alias);
			};

			scope.trayClick = function(){
				navigationService.showTray();
			};

        }
    };
}

angular.module('umbraco.directives').directive("umbSections", sectionsDirective);
