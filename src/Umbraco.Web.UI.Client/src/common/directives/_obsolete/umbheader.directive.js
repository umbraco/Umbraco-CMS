/**
* @ngdoc directive
* @name umbraco.directives.directive:umbHeader
* @deprecated
* We plan to remove this directive in the next major version of umbraco (8.0). The directive is not recommended to use.
* @restrict E
* @function
* @description
* The header on an editor that contains tabs using bootstrap tabs - THIS IS OBSOLETE, use umbTabHeader instead
**/

angular.module("umbraco.directives")
.directive('umbHeader', function ($parse, $timeout) {
    return {
        restrict: 'E',
        replace: true,
        transclude: 'true',
        templateUrl: 'views/directives/_obsolete/umb-header.html',
        //create a new isolated scope assigning a tabs property from the attribute 'tabs'
        //which is bound to the parent scope property passed in
        scope: {
            tabs: "="
        },
        link: function (scope, iElement, iAttrs) {

            scope.showTabs = iAttrs.tabs ? true : false;
            scope.visibleTabs = [];

            //since tabs are loaded async, we need to put a watch on them to determine
            // when they are loaded, then we can close the watch
            var tabWatch = scope.$watch("tabs", function (newValue, oldValue) {

                angular.forEach(newValue, function(val, index){
                        var tab = {id: val.id, label: val.label};
                        scope.visibleTabs.push(tab);
                });

                //don't process if we cannot or have already done so
                if (!newValue) {return;}
                if (!newValue.length || newValue.length === 0){return;}

                //we need to do a timeout here so that the current sync operation can complete
                // and update the UI, then this will fire and the UI elements will be available.
                $timeout(function () {

                    //use bootstrap tabs API to show the first one
                    iElement.find(".nav-tabs a:first").tab('show');

                    //enable the tab drop
                    iElement.find('.nav-pills, .nav-tabs').tabdrop();

                    //ensure to destroy tabdrop (unbinds window resize listeners)
                    scope.$on('$destroy', function () {
                        iElement.find('.nav-pills, .nav-tabs').tabdrop("destroy");
                    });

                    //stop watching now
                    tabWatch();
                }, 200);

            });
        }
    };
});
