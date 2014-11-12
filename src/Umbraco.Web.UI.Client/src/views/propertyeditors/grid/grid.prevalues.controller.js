angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.GridPrevalueEditorController",
    function ($scope, $http, assetsService, $rootScope, dialogService, mediaResource, gridService, imageHelper, $timeout) {

        var emptyModel = {
            styles:[
                {
                    label: "Set a background image",
                    description: "Set a row background",
                    key: "background-image",
                    view: "imagepicker",
                    modifier: "url({0})"
                }
            ],

            config:[
                {
                    label: "Class",
                    description: "Set a css class",
                    key: "class",
                    view: "textstring"
                }
            ],

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
           if(template === undefined){
                template = {
                    name: "",
                    sections:[

                    ]
                };
                $scope.model.value.templates.push(template);
           }    
           
           dialogService.open(
               {
                   template: "views/propertyEditors/grid/dialogs/layoutconfig.html",
                   currentLayout: template,
                   rows: $scope.model.value.layouts,
                   columns: $scope.model.value.columns
               }
           );

        };
        $scope.deleteTemplate = function(index){
            $scope.model.value.templates.splice(index, 1);
        };
        

        




        /****************
            Row
        *****************/

        $scope.configureLayout = function(layout){

            if(layout === undefined){
                 layout = {
                     name: "",
                     areas:[

                     ]
                 };
                 $scope.model.value.layouts.push(layout);
            }

            dialogService.open(
                {
                    template: "views/propertyEditors/grid/dialogs/rowconfig.html",
                    currentRow: layout,
                    editors: $scope.editors,
                    columns: $scope.model.value.columns
                }
            );
        };


        $scope.deleteLayout = function(index){
            $scope.model.value.layouts.splice(index, 1);
        };


        


        /****************
            utillities
        *****************/
        $scope.scaleUp = function(section, max, overflow){
           var add = 1;
           if(overflow !== true){
                add = (max > 1) ? 1 : max;
           }
           //var add = (max > 1) ? 1 : max;
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


        $scope.removeConfigValue = function(collection, index){
            collection.splice(index, 1);
        };


        var editConfigCollection = function(configValues, title, callbackOnSave){
            dialogService.open(
                {
                    template: "views/propertyeditors/grid/dialogs/editconfig.html",
                    config: configValues,
                    name: title,
                    callback: function(data){
                        callbackOnSave(data);
                    }
                });
        };

        $scope.editConfig = function(){
            editConfigCollection($scope.model.value.config, "Settings", function(data){
                $scope.model.value.config = data;
            });
	    };

        $scope.editStyles = function(){
            editConfigCollection($scope.model.value.styles, "Styling", function(data){
                $scope.model.value.styles = data;
            });
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


            if (!$scope.model.value.config) {
                $scope.model.value.config = [];
            }

            if (!$scope.model.value.styles) {
                $scope.model.value.styles = [];
            }
        }

    });
