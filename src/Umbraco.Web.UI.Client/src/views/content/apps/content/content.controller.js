(function () {
    "use strict";

    function ContentAppContentController($scope, $timeout) {

        var vm = this;
        vm.loading = true;

        function onInit() {

            //TODO: This is pretty ugly since this component inherits the scope from umbEditorSubViews which is supplied a
            //'model' which is the entire content object passed from the server which we can't use because this 'app' needs to be
            //in the context of the current culture within a split view. So the content controller will assign a special 'viewModel' to the
            //subView so that we have a model in the context of the editor.
            //Ideally this would be a directive and we can just pass a model in but because contentApps currently are
            //rendered purely based on views that won't work. Perhaps we can consider dynamically compiling directives like
            // https://www.codelord.net/2015/05/19/angularjs-dynamically-loading-directives/

            vm.content = $scope.subView.viewModel;

            angular.forEach(vm.content.tabs, function (group) {
                group.open = true;
            });

            vm.loading = false;
        }

        onInit();

        //if this variant has a culture/language assigned, then we need to watch it since it will change
        //if the language drop down changes and we need to re-init
        if ($scope.subView.viewModel.language) {
            $scope.$watch(function () {
                return $scope.subView.viewModel.language.culture;
            }, function (newVal, oldVal) {
                if (newVal !== oldVal) {
                    vm.loading = true;

                    //TODO: Can we minimize the flicker?
                    $timeout(function () {
                        onInit();
                    }, 100);
                }
            });
        }
    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.Apps.ContentController", ContentAppContentController);
})();
