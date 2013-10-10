/**
* @ngdoc directive
* @name umbraco.directives.directive:umbSections
* @restrict E
**/
function sectionsDirective($timeout, $window, navigationService, sectionResource) {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/directives/umb-sections.html',
        link: function (scope, element, attr, ctrl) {
			
			scope.maxSections = 7;
			scope.overflowingSections = 0;
            scope.sections = [];

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

			scope.sectionClick = function(section){
				navigationService.showTree(section.alias);
			};

			scope.sectionDblClick = function(section){
				navigationService.changeSection(section.alias);
			};

			scope.trayClick = function(){
				navigationService.showTray();
			};

        }
    };
}

angular.module('umbraco.directives').directive("umbSections", sectionsDirective);
