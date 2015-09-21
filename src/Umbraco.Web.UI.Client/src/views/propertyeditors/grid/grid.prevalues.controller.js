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

        $scope.configureTemplate = function(template) {

           var templatesCopy = angular.copy($scope.model.value.templates);

           if (template === undefined) {
              template = {
                 name: "",
                 sections: [

                 ]
              };
              $scope.model.value.templates.push(template);
           }

           $scope.layoutConfigOverlay = {};
           $scope.layoutConfigOverlay.view = "views/propertyEditors/grid/dialogs/layoutconfig.html";
           $scope.layoutConfigOverlay.currentLayout = template;
           $scope.layoutConfigOverlay.rows = $scope.model.value.layouts;
           $scope.layoutConfigOverlay.columns = $scope.model.value.columns;
           $scope.layoutConfigOverlay.show = true;

           $scope.layoutConfigOverlay.submit = function(model) {
              $scope.layoutConfigOverlay.show = false;
              $scope.layoutConfigOverlay = null;
           };

           $scope.layoutConfigOverlay.close = function(oldModel) {

              //reset templates
              $scope.model.value.templates = templatesCopy;

              $scope.layoutConfigOverlay.show = false;
              $scope.layoutConfigOverlay = null;
           }

        };

        $scope.deleteTemplate = function(index){
            $scope.model.value.templates.splice(index, 1);
        };
        

        /****************
            Row
        *****************/

        $scope.configureLayout = function(layout) {

           var layoutsCopy = angular.copy($scope.model.value.layouts);

           if(layout === undefined){
                layout = {
                    name: "",
                    areas:[

                    ]
                };
                $scope.model.value.layouts.push(layout);
           }

           $scope.rowConfigOverlay = {};
           $scope.rowConfigOverlay.view = "views/propertyEditors/grid/dialogs/rowconfig.html";
           $scope.rowConfigOverlay.currentRow = layout;
           $scope.rowConfigOverlay.editors = $scope.editors;
           $scope.rowConfigOverlay.columns = $scope.model.value.columns;
           $scope.rowConfigOverlay.show = true;

           $scope.rowConfigOverlay.submit = function(model) {
             $scope.rowConfigOverlay.show = false;
             $scope.rowConfigOverlay = null;
           };

           $scope.rowConfigOverlay.close = function(oldModel) {
             $scope.model.value.layouts = layoutsCopy;
             $scope.rowConfigOverlay.show = false;
             $scope.rowConfigOverlay = null;
           };

        };

        //var rowDeletesPending = false;
        $scope.deleteLayout = function(index) {

           $scope.rowDeleteOverlay = {};
           $scope.rowDeleteOverlay.view = "views/propertyEditors/grid/dialogs/rowdeleteconfirm.html";
           $scope.rowDeleteOverlay.dialogData = {
             rowName: $scope.model.value.layouts[index].name
           };
           $scope.rowDeleteOverlay.show = true;

           $scope.rowDeleteOverlay.submit = function(model) {

             $scope.model.value.layouts.splice(index, 1);

             $scope.rowDeleteOverlay.show = false;
             $scope.rowDeleteOverlay = null;
           };

           $scope.rowDeleteOverlay.close = function(oldModel) {
             $scope.rowDeleteOverlay.show = false;
             $scope.rowDeleteOverlay = null;
           };

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
            return ((spans / $scope.model.value.columns) * 100).toFixed(8);
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

        var editConfigCollection = function(configValues, title, callback) {

           $scope.editConfigCollectionOverlay = {};
           $scope.editConfigCollectionOverlay.view = "views/propertyeditors/grid/dialogs/editconfig.html";
           $scope.editConfigCollectionOverlay.config = configValues;
           $scope.editConfigCollectionOverlay.title = title;
           $scope.editConfigCollectionOverlay.show = true;

           $scope.editConfigCollectionOverlay.submit = function(model) {

              callback(model.config)

              $scope.editConfigCollectionOverlay.show = false;
              $scope.editConfigCollectionOverlay = null;
           };

           $scope.editConfigCollectionOverlay.close = function(oldModel) {
              $scope.editConfigCollectionOverlay.show = false;
              $scope.editConfigCollectionOverlay = null;
           };

        };

        $scope.editConfig = function() {
           editConfigCollection($scope.model.value.config, "Settings", function(data) {
              $scope.model.value.config = data;
           });
        };

        $scope.editStyles = function() {
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
