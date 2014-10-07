angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.GridPrevalueEditorController",
    function ($scope, $http, assetsService, $rootScope, dialogService, mediaResource, gridService, imageHelper, $timeout) {

        var emptyModel = {
            columns: 12,
            templates:[
                {
                    name: "1 column layout",
                    sections: [
                        {
                            grid: 12,
                        }
                    ]
                },
                {
                    name: "2 column layout",
                    sections: [
                        {
                            grid: 4,
                        },
                        {
                            grid: 8
                        }
                    ]
                }
            ],


            layouts:[
                {
                    name: "Headline",
                    areas: [
                        {
                            grid: 12,
                            editors: ["headline"]
                        }
                    ]
                },
                {
                    name: "Article",
                    areas: [
                        {
                            grid: 4
                        },
                        {
                            grid: 8
                        }
                    ]
                }
            ]
        };

        /****************
            template
        *****************/

        $scope.configureTemplate = function(template){
           if($scope.currentTemplate && $scope.currentTemplate === template){
                delete $scope.currentTemplate;
           }else{
               //if no template is passed in, we can assume we are adding a new one
               if(template === undefined){
                    template = {
                        name: "",
                        sections:[

                        ]
                    };
                    $scope.model.value.templates.push(template);
               }
               $scope.currentTemplate = template;
            }
        };
        $scope.deleteTemplate = function(index){
            $scope.model.value.templates.splice(index, 1);
        };
        $scope.closeTemplate = function(){

           //clean-up
           _.forEach($scope.currentTemplate.sections, function(section, index){
                if(section.grid <= 0){
                    $scope.currentTemplate.sections.splice(index, 1);
                }
           });

           $scope.currentTemplate = undefined;

        };

        /****************
            Section
        *****************/
        $scope.configureSection = function(section, template){
            if($scope.currentSection && $scope.currentSection === section){
                delete $scope.currentSection;
            }else{
               if(section === undefined){
                    var space = ($scope.availableTemplateSpace > 4) ? 4 : $scope.availableTemplateSpace;
                    section = {
                        grid: space
                    };
                    template.sections.push(section);
               }
               $scope.currentSection = section;
            }
        };
        $scope.deleteSection = function(index){
            $scope.currentTemplate.sections.splice(index, 1);
        };
        $scope.closeSection = function(){
            $scope.currentSection = undefined;
        };




        /****************
            layout
        *****************/

        $scope.configureLayout = function(layout){
           if($scope.currentLayout && $scope.currentLayout === layout){
                delete $scope.currentLayout;
           }else{
               //if no template is passed in, we can assume we are adding a new one
               if(layout === undefined){
                    layout = {
                        name: "",
                        areas:[

                        ]
                    };
                    $scope.model.value.layouts.push(layout);
               }
               $scope.currentLayout = layout;
            }
        };
        $scope.deleteLayout = function(index){
            $scope.model.value.layouts.splice(index, 1);
        };
        $scope.closeLayout = function(){

           //clean-up
           _.forEach($scope.currentLayout.areas, function(area, index){
                if(area.grid <= 0){
                    $scope.currentLayout.areas.splice(index, 1);
                }
           });

           $scope.currentLayout = undefined;
        };


        /****************
            area
        *****************/
        $scope.configureArea = function(area, layout){
            if($scope.currentArea && $scope.currentArea === area){
                delete $scope.currentArea;
            }else{
               if(area === undefined){
                    var space = ($scope.availableLayoutSpace > 4) ? 4 : $scope.availableLayoutSpace;
                    area = {
                        grid: space
                    };
                    layout.areas.push(area);
               }
               $scope.currentArea = area;
            }
        };
        $scope.deleteArea = function(index){
            $scope.currentLayout.areas.splice(index, 1);
        };
        $scope.closeArea = function(){
            $scope.currentArea = undefined;
        };


        /****************
            utillities
        *****************/
        $scope.scaleUp = function(section, max){
           var add = (max > 1) ? 1 : max;
           section.grid = section.grid+add;
        };
        $scope.scaleDown = function(section){
           var remove = (section.grid > 1) ? 1 : section.grid;
           section.grid = section.grid-remove;
        };
        $scope.toggleCollection = function(collection, toggle){
            if(toggle){
                collection = [];
            }else{
                delete collection;
            }
        };
        
        $scope.percentage = function(spans){
            return ((spans / $scope.model.value.columns) * 100).toFixed(1);
        };

        /****************
            watchers
        *****************/
        $scope.$watch("currentTemplate", function(template){
            if(template){
                var total = 0;
                _.forEach(template.sections, function(section){
                    total = (total + section.grid);
                });
                $scope.availableTemplateSpace = $scope.model.value.columns - total;
            }
        }, true);

        $scope.$watch("currentLayout", function(layout){
            if(layout){
                var total = 0;
                _.forEach(layout.areas, function(area){
                    total = (total + area.grid);
                });
                $scope.availableLayoutSpace = $scope.model.value.columns - total;
            }
        }, true);


        /****************
            editors
        *****************/
        gridService.getGridEditors().then(function(response){
            $scope.editors = response.data;
        });


        /* init grid data */
        if (!$scope.model.value || $scope.model.value === "" || !$scope.model.value.templates) {
            $scope.model.value = emptyModel;
        } else {
            if (!$scope.model.value.columns) {
                $scope.model.value.columns = emptyModel.columns;
            }
        }

    });
