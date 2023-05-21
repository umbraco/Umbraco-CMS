(function () {
    "use strict";

    function SectionPickerController($scope, sectionResource, localizationService) {
        
        var vm = this;

        vm.sections = [];
        vm.loading = false;

        vm.selectSection = selectSection;
        vm.submit = submit;
        vm.close = close;

        //////////

        function onInit() {

            vm.loading = true;

            // set default title
            if(!$scope.model.title) {
                localizationService.localize("defaultdialogs_selectSections").then(function(value){
                    $scope.model.title = value;
                });
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
            selection.forEach(function(selected){
                vm.sections.forEach(function(section){
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

                $scope.model.selection.forEach(function(selectedSection, index){
                    if(selectedSection.alias === section.alias) {
                        section.selected = false;
                        $scope.model.selection.splice(index, 1);
                    }
                });

            }

        }

        function setSectionIcon(sections) {
            sections.forEach(function(section) {
                section.icon = "icon-section";
            });
        }

        function submit(model) {
            if($scope.model.submit) {
                $scope.model.submit(model);
            }
        }

        function close() {
            if($scope.model.close) {
                $scope.model.close();
            }
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.SectionPickerController", SectionPickerController);

})();
