
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
                hideNoResult: "<?",
                warningText: "<?",
                loading : "=?"
            }
        });

    function UmbTrackedReferencesController($q, trackedReferencesResource) {

      
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

            this.loading = this.loading || true;
            this.hideNoResult = this.hideNoResult || false;
            this.warningText = this.warningText || '';

            vm.showWarning = false;
            vm.hasWarningText = this.warningText !== '';

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
                        descendantsPromises.push(checkMemberDescendantsUsage());
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

        function checkContentDescendantsUsage() {
           return trackedReferencesResource.hasReferencesInDescendants(vm.id, vm.contentOptions.entityType)
               .then(function (data) {
                   vm.hasContentReferencesInDescendants = data;

                   if (vm.hasContentReferencesInDescendants) {
                       activateWarning();
                   }
                });
        }

        function checkMediaDescendantsUsage() {
            return trackedReferencesResource.hasReferencesInDescendants(vm.id, vm.mediaOptions.entityType)
                .then(function (data) {
                    vm.hasMediaReferencesInDescendants = data;

                    if (vm.hasMediaReferencesInDescendants) {
                        activateWarning();
                    }
                });
        }

        function checkMemberDescendantsUsage() {
            return trackedReferencesResource.hasReferencesInDescendants(vm.id, vm.memberOptions.entityType)
                .then(function (data) {
                    vm.hasMemberReferencesInDescendants = data;

                    if (vm.hasMemberReferencesInDescendants) {
                        activateWarning();
                    }
                });
        }

        function activateWarning() {
            if (vm.hasWarningText && !vm.showWarning) {
                vm.showWarning = true;
            }
        }
    }

})();
