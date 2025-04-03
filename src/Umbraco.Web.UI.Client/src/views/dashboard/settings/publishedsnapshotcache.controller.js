function publishedSnapshotCacheController($scope, $http, umbRequestHelper, localizationService, overlayService) {

    var vm = this;

    vm.collect = collect;
    vm.reload = reload;
    vm.verify = verify;
    vm.rebuild = rebuild;

    function reload(event) {
        if (vm.working) return;

        const dialog = {
            view: "views/dashboard/settings/overlays/nucache.reload.html",
            submitButtonLabelKey: "general_ok",
            submit: function (model) {
                performReload();
                overlayService.close();
            },
            close: function () {
                overlayService.close();
            }
        };

        localizationService.localize("general_reload").then(value => {
            dialog.title = value;
            overlayService.open(dialog);
        });

        event.preventDefault()
        event.stopPropagation();
    }

    function collect() {
        if (vm.working) return;
        vm.working = true;
        umbRequestHelper.resourcePromise(
            $http.get(umbRequestHelper.getApiUrl("publishedSnapshotCacheStatusBaseUrl", "Collect")),
                    'Failed to verify the cache.')
            .then(function (result) {
                vm.working = false;
                vm.status = result;
            });
    }

    function verify() {
        if (vm.working) return;
        vm.working = true;
        umbRequestHelper.resourcePromise(
            $http.get(umbRequestHelper.getApiUrl("publishedSnapshotCacheStatusBaseUrl", "GetStatus")),
                    'Failed to verify the cache.')
            .then(function (result) {
                vm.working = false;
                vm.status = result;
            });
    }

    function rebuild(event) {
        if (vm.working) return;

        const dialog = {
            view: "views/dashboard/settings/overlays/nucache.rebuild.html",
            submitButtonLabelKey: "general_ok",
            submit: function (model) {
                performRebuild();
                overlayService.close();
            },
            close: function () {
                overlayService.close();
            }
        };

        localizationService.localize("general_rebuild").then(value => {
            dialog.title = value;
            overlayService.open(dialog);
        });

        event.preventDefault()
        event.stopPropagation();
    }

    function performReload() {
        vm.working = true;

        umbRequestHelper.resourcePromise(
            $http.post(umbRequestHelper.getApiUrl("publishedSnapshotCacheStatusBaseUrl", "ReloadCache")),
            'Failed to trigger a cache reload')
            .then(function (result) {
                vm.working = false;
            });
    }

    function performRebuild() {
        vm.working = true;

        umbRequestHelper.resourcePromise(
            $http.post(umbRequestHelper.getApiUrl("publishedSnapshotCacheStatusBaseUrl", "RebuildDbCacheInBackground")), "Failed to queue the rebuild task.")
                .then(function () {
                    const interval = setInterval(function () {
                        $http.get(umbRequestHelper.getApiUrl("publishedSnapshotCacheStatusBaseUrl", "GetStatus"))
                            .then(function (result) {
                                if (!result.data.toString().startsWith("Rebuild in progress")) {
                                    vm.working = false;
                                    vm.status = result.data;
                                    clearInterval(interval);
                                }
                          });

                        }, 2000);
                });
    }

    function init() {
        vm.working = false;
        verify();
    }

    init();
}
angular.module("umbraco").controller("Umbraco.Dashboard.PublishedSnapshotCacheController", publishedSnapshotCacheController);
