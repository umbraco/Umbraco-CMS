
(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbTrackedReferences', {
            transclude: true,
            templateUrl: 'views/components/references/umb-tracked-references.html',
            controller: UmbTrackedReferencesController,
            controllerAs: 'vm',
            bindings: {
                id: "<",
                loading : "=?"
            }
        });

    function UmbTrackedReferencesController($q, trackedReferencesResource) {

        this.loading = this.loading || true;

        var vm = this;

        vm.changeContentPageNumber = changeContentPageNumber;
        vm.contentOptions = {};
        vm.contentOptions.entityType = "DOCUMENT";
        vm.hasContentReferences = false;
        vm.hasContentReferencesInDescendants = false;

        vm.changeMediaPageNumber = changeMediaPageNumber;
        vm.mediaOptions = {};
        vm.mediaOptions.entityType = "MEDIA";
        vm.hasMediaReferences = false;
        vm.hasMediaReferencesInDescendants = false;

        vm.changeMemberPageNumber = changeMemberPageNumber;
        vm.memberOptions = {};
        vm.memberOptions.entityType = "MEMBER";
        vm.hasMemberReferences = false;
        vm.hasMemberReferencesInDescendants = false;

        vm.$onInit = onInit;

        function onInit() {

            $q.all([loadContentRelations(), loadMediaRelations(), loadMemberRelations()]).then(function () {
                
                if (vm.hasContentReferences && vm.hasMediaReferences && vm.hasMemberReferences) {
                    vm.loading = false;
                } else {
                    var descendantsPromises = [];

                    if (!vm.hasContentReferences) {
                        descendantsPromises.push(checkContentDescendantsUsage());
                    }

                    if (!vm.hasMediaReferences) {
                        descendantsPromises.push(checkMediaDescendantsUsage());
                    }

                    if (!vm.hasMemberReferences) {
                        descendantsPromises.push(checkMemberDescendantsUsage())
                    }

                    $q.all(descendantsPromises).then(function() {
                        vm.loading = false;
                    });

                }
            });

           
        }

        function changeContentPageNumber(pageNumber) {
            vm.contentOptions.pageNumber = pageNumber;
            loadContentRelations();
        }

        function changeMediaPageNumber(pageNumber) {
            vm.mediaOptions.pageNumber = pageNumber;
            loadMediaRelations();
        }

        function changeMemberPageNumber(pageNumber) {
            vm.memberOptions.pageNumber = pageNumber;
            loadMemberRelations();
        }

        function loadContentRelations() {
            return trackedReferencesResource.getPagedReferences(vm.id, vm.contentOptions)
                .then(function (data) {
                    vm.contentReferences = data;
                    vm.hasContentReferences = data.items.length > 0;
                });
        }

        function loadMediaRelations() {
            return trackedReferencesResource.getPagedReferences(vm.id, vm.mediaOptions)
                .then(function (data) {
                    vm.mediaReferences = data;
                    vm.hasMediaReferences = data.items.length > 0;
                });
        }

        function loadMemberRelations() {
            return trackedReferencesResource.getPagedReferences(vm.id, vm.memberOptions)
                .then(function (data) {
                    vm.memberReferences = data;
                    vm.hasMemberReferences = data.items.length > 0;
                });
        }

        function checkContentDescendantsUsage() {
           return trackedReferencesResource.hasReferencesInDescendants(vm.id, vm.contentOptions.entityType)
                .then(function (data) {
                    vm.hasContentReferencesInDescendants = data;
                });
        }

        function checkMediaDescendantsUsage() {
            return trackedReferencesResource.hasReferencesInDescendants(vm.id, vm.mediaOptions.entityType)
                .then(function (data) {
                    vm.hasMediaReferencesInDescendants = data;
                });
        }

        function checkMemberDescendantsUsage() {
            return trackedReferencesResource.hasReferencesInDescendants(vm.id, vm.memberOptions.entityType)
                .then(function (data) {
                    vm.hasMemberReferencesInDescendants = data;
                });
        }
    }

})();
