function ProfilerController($scope, $cookies, $http, umbRequestHelper) {
	var vm = this;

	vm.loading = true;
	vm.toggle = toggle;

	function toggle() {
		if (vm.alwaysOn === true) {
            $cookies.remove("UMB-DEBUG", {
                path: "/"
            });
			vm.alwaysOn = false;
		}
		else {
			$cookies.put("UMB-DEBUG", "true", {
                path: "/",
                expires: "Tue, 01 Jan 2100 00:00:01 GMT"
            });
			vm.alwaysOn = true;
		}
	}

	function init() {
        vm.alwaysOn = $cookies.get("UMB-DEBUG") === "true";

        umbRequestHelper.resourcePromise(
		    $http.get(umbRequestHelper.getApiUrl("webProfilingBaseUrl", "GetStatus")),
			"Failed to retrieve status for web profiling"
		).then(function(status) {
			vm.loading = false;
			vm.profilerEnabled = status.Enabled;
		});
	}

	init();
}

angular.module("umbraco").controller("Umbraco.Dashboard.ProfilerController", ProfilerController);
