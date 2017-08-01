(function () {
    "use strict";

    function SectionPickerController($scope, sectionResource, localizationService) {
        
        var vm = this;

        vm.sections = [];
        vm.loading = false;

        vm.selectSection = selectSection;

        //////////

        function onInit() {

            vm.loading = true;

            // set default title
            if(!$scope.model.title) {
                $scope.model.title = localizationService.localize("defaultdialogs_selectSections");
            }

            // make sure we can push to something
            if(!$scope.model.selection) {
                $scope.model.selection = [];
            }

            // get sections
            sectionResource.getAllSections().then(function(sections){
                vm.sections = sections;

                setSectionIcon(vm.sections);
                
                if($scope.model.selection && $scope.model.selection.length > 0) {
                    preSelect($scope.model.selection);
                }
                
                vm.loading = false;

            });
            
        }

        function preSelect(selection) {
            angular.forEach(selection, function(selected){
                angular.forEach(vm.sections, function(section){
                    if(selected.alias === section.alias) {
                        section.selected = true;
                    }
                });
            });
        }

        function selectSection(section) {

            if(!section.selected) {
                
                section.selected = true;
                $scope.model.selection.push(section);

            } else {

                angular.forEach($scope.model.selection, function(selectedSection, index){
                    if(selectedSection.alias === section.alias) {
                        section.selected = false;
                        $scope.model.selection.splice(index, 1);
                    }
                });

            }

        }

        function setSectionIcon(sections) {
            angular.forEach(sections, function(section) {
                section.icon = "icon-section " + section.cssclass;
            });
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.SectionPickerController", SectionPickerController);

})();
