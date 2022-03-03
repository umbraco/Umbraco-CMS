(function () {
    'use strict';

    /**
     * A component to render the tracked references of an item
     */

    function umbTrackedReferencesController($q, trackedReferencesResource, localizationService) {

        var vm = this;

        vm.contentReferencesTitle = "Referenced by the following Documents";
        vm.memberReferencesTitle = "Referenced by the following Members";
        vm.mediaReferencesTitle = "Referenced by the following Media";
        vm.referencedDescendantsTitle = "The following descendant items are referenced";

        localizationService.localize("references_labelUsedByDocuments").then(function (value) {
            vm.contentReferencesTitle = value;
        });

        localizationService.localize("references_labelUsedByMembers").then(function (value) {
            vm.memberReferencesTitle = value;
        });

        localizationService.localize("references_labelUsedByMedia").then(function (value) {
            vm.mediaReferencesTitle = value;
        });

        localizationService.localize("references_labelUsedDescendants").then(function (value) {
            vm.referencedDescendantsTitle = value;
        });
      
        vm.changeContentPageNumber = changeContentPageNumber;
        vm.changeContentDescendantsPageNumber = changeContentDescendantsPageNumber;
        vm.contentOptions = {};
        vm.contentOptions.entityType = "DOCUMENT";
        vm.hasContentReferences = false;
        vm.hasContentReferencesInDescendants = false;

        vm.changeMediaPageNumber = changeMediaPageNumber;
        vm.changeMediaDescendantsPageNumber = changeMediaDescendantsPageNumber;
        vm.mediaOptions = {};
        vm.mediaOptions.entityType = "MEDIA";
        vm.hasMediaReferences = false;
        vm.hasMediaReferencesInDescendants = false;

        vm.changeMemberPageNumber = changeMemberPageNumber;
        vm.memberOptions = {};
        vm.memberOptions.entityType = "MEMBER";
        vm.hasMemberReferences = false;

        vm.$onInit = onInit;

        function onInit() {

            this.loading = true;
            this.hideNoResult = this.hideNoResult || false;

            // when vm.id == 0 it means that this is a new item, so it has no references yet
            if (vm.id === 0) {
                vm.loading = false;
                if(vm.onLoadingComplete) {
                    vm.onLoadingComplete();
                }
                return;
            }

            // Make array of promises to load:
            var promises = [loadContentRelations(), loadMediaRelations(), loadMemberRelations()];
            
            // only load descendants if we want to show them.
            if (vm.showDescendants) {
                promises.push(loadContentDescendantsUsage());
                promises.push(loadMediaDescendantsUsage());
            }

            $q.all(promises).then(function () {
                vm.loading = false;
                if(vm.onLoadingComplete) {
                    vm.onLoadingComplete();
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

        function changeContentDescendantsPageNumber(pageNumber) {
            vm.contentOptions.pageNumber = pageNumber;
            loadContentDescendantsUsage();
        }

        function changeMediaDescendantsPageNumber(pageNumber) {
            vm.mediaOptions.pageNumber = pageNumber;
            loadMediaDescendantsUsage();
        }

        function loadContentRelations() {
            return trackedReferencesResource.getPagedReferences(vm.id, vm.contentOptions)
                .then(function (data) {
                    vm.contentReferences = data;

                    if (data.items.length > 0) {
                        vm.hasContentReferences = data.items.length > 0;
                        activateWarning();
                    }
                });
        }

        function loadMediaRelations() {
            return trackedReferencesResource.getPagedReferences(vm.id, vm.mediaOptions)
                .then(function (data) {
                    vm.mediaReferences = data;

                    if (data.items.length > 0) {
                        vm.hasMediaReferences = data.items.length > 0;
                        activateWarning();
                    }
                });
        }

        function loadMemberRelations() {
            return trackedReferencesResource.getPagedReferences(vm.id, vm.memberOptions)
                .then(function (data) {
                    vm.memberReferences = data;

                    if (data.items.length > 0) {
                        vm.hasMemberReferences = data.items.length > 0;
                        activateWarning();
                    }
                });
        }

        function loadContentDescendantsUsage() {
            return trackedReferencesResource.getPagedDescendantsInReferences(vm.id, vm.contentOptions)
                .then(function (data) {
                    vm.referencedContentDescendants = data;

                    if (data.items.length > 0) {
                        vm.hasContentReferencesInDescendants = data.items.length > 0;
                        activateWarning();
                    }
                });
        }

        function loadMediaDescendantsUsage() {
            return trackedReferencesResource.getPagedDescendantsInReferences(vm.id, vm.mediaOptions)
                .then(function (data) {
                    vm.referencedMediaDescendants = data;

                    if (data.items.length > 0) {
                        vm.hasMediaReferencesInDescendants = data.items.length > 0;
                        activateWarning();
                    }
                });
        }

        function activateWarning() {
            if (vm.onWarning) {
                vm.onWarning();
            }
        }
    }

    var umbTrackedReferencesComponent = {
        templateUrl: 'views/components/references/umb-tracked-references.html',
        transclude: true,
        bindings: {
            id: "<",
            hideNoResult: "<?",
            onWarning: "&?",
            onLoadingComplete: "&?",
            compact: "<?",
            showDescendants: "<?"
        },
        controllerAs: 'vm',
        controller: umbTrackedReferencesController
    };

    angular.module('umbraco.directives').component('umbTrackedReferences', umbTrackedReferencesComponent);

})();
