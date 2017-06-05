function helpDrawer($location, $routeParams, helpService, userService, localizationService, dashboardResource) {

    return {

        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/components/application/umb-help-drawer.html',
        transclude: true,
       
        link: function (scope, element, attr, ctrl) {

            scope.model = {};
            scope.model.title = localizationService.localize("general_help");
            scope.model.subtitle = "Umbraco version" + " " + Umbraco.Sys.ServerVariables.application.version + " assembly: " + Umbraco.Sys.ServerVariables.application.assemblyVersion;
            scope.model.section = $routeParams.section;

            if (!scope.model.section) {
                scope.model.section = "content";
            }

            
            dashboardResource.getDashboard("user-help").then(function (dashboard) {
                scope.model.dashboard = dashboard;
            });
           
        }

    };
}

angular.module('umbraco.directives').directive("umbHelpDrawer", helpDrawer);