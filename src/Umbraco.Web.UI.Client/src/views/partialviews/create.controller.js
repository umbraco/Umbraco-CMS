(function () {
    "use strict";

    function PartialViewsCreateController($scope, codefileResource, $location, navigationService) {

        var vm = this;
        var node = $scope.dialogOptions.currentNode;

        vm.snippets = [];
        vm.showSnippets = false;
        vm.creatingFolder = false;

        vm.createPartialView = createPartialView;
        vm.showCreateFolder = showCreateFolder;
        vm.createFolder = createFolder;
        vm.showCreateFromSnippet = showCreateFromSnippet;

        function onInit() {
            codefileResource.getSnippets('partialViews')
                .then(function(snippets) {
                    vm.snippets = snippets;
                });
        }

        function createPartialView(selectedSnippet) {

            var snippet = null;

            if(selectedSnippet && selectedSnippet.fileName) {
                snippet = selectedSnippet.fileName;
            }

            $location.search('create', null);
            $location.search('snippet', null);
            $location.path("/settings/partialviews/edit/" + node.id).search("create", "true").search("snippet", snippet);
            navigationService.hideMenu();

        }

        function showCreateFolder() {
            vm.creatingFolder = true;
        }

        function createFolder() {

        }
        
        function showCreateFromSnippet() {
            vm.showSnippets = true;
        }
        
        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.PartialViews.CreateController", PartialViewsCreateController);
})();
