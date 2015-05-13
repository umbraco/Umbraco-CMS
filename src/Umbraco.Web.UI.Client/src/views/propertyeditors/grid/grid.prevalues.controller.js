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

        //var rowDeletesPending = false;
        $scope.deleteLayout = function (index) {
            //rowDeletesPending = true;

            //show ok/cancel dialog
            var confirmDialog = dialogService.open(
               {
                   template: "views/propertyEditors/grid/dialogs/rowdeleteconfirm.html",
                   show: true,
                   callback: function() {
                       $scope.model.value.layouts.splice(index, 1);
                   },
                   dialogData: {
                       rowName: $scope.model.value.layouts[index].name
                   }
               }
           );
            
        };
        


        /****************
            utillities
        *****************/
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

        $scope.zeroWidthFilter = function (cell) {
                return cell.grid > 0;
        };

        /****************
            Config
        *****************/

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

        /****************
            Clean up
        *****************/
        var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
            var ts = $scope.model.value.templates;
            var ls = $scope.model.value.layouts;

            _.each(ts, function(t){
                _.each(t.sections, function(section, index){
                   if(section.grid === 0){
                    t.sections.splice(index, 1);
                   }
               });
            });

            _.each(ls, function(l){
                _.each(l.areas, function(area, index){
                   if(area.grid === 0){
                    l.areas.splice(index, 1);
                   }
               });
            });
        });

        //when the scope is destroyed we need to unsubscribe
        $scope.$on('$destroy', function () {
            unsubscribe();
        });

    });
