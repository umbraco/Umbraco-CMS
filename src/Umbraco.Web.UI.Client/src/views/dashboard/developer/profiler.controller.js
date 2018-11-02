(function() {
    "use strict";

    function ProfilerController($scope, $cookieStore, $http, umbRequestHelper) {
        var COOKIE = "UMB-DEBUG";

        var vm = this;

        vm.loading = true;
        vm.toggle = toggle;

        function toggle() {
            // NOTE: $cookieStore.put/remove seems broken with the version of Angular used in V7. Might work better in V8.
            if (vm.alwaysOn === true) {
                //$cookieStore.remove(COOKIE);
                document.cookie = COOKIE + "=; expires=Thu, 01 Jan 1970 00:00:01 GMT;path=/";
                vm.alwaysOn = false;
            }
            else {
                //$cookieStore.put(COOKIE, true);
                document.cookie = COOKIE + "=true; expires=Tue, 01 Jan 2030 00:00:01 GMT;path=/";
                vm.alwaysOn = true;
            }

        }

        function init() {
            vm.alwaysOn = $cookieStore.get(COOKIE) === true;

            umbRequestHelper.resourcePromise(
                $http.get(Umbraco.Sys.ServerVariables.umbracoUrls.webProfilingBaseUrl + "GetStatus"),
                "Failed to retrieve status for web profiling"
            ).then(function(status) {
                vm.loading = false;
                vm.profilerEnabled = status.Enabled;
            });
        }

        init();
    }

    angular.module("umbraco").controller("Umbraco.Dashboard.ProfilerController", ProfilerController);
})();
