angular.module("umbraco").controller("Umbraco.PrevalueEditors.RteController",
    function ($scope, $timeout, tinyMceService, stylesheetResource) {
        var cfg = {};
        cfg.toolbar = ["code", "bold", "italic", "umbracocss","alignleft", "aligncenter", "alignright", "bullist","numlist", "outdent", "indent", "link", "image", "umbmediapicker", "umbembeddialog", "umbmacro"];
        cfg.stylesheets = [];
        cfg.dimensions = {height: 400, width: 600};

        if($scope.model.value){
            if(angular.isString($scope.model.value)){
                $scope.model.value = angular.fromJson($scope.model.value);
            }
            angular.extend($scope.model.value, cfg);
        }else{
            $scope.model.value = cfg;
        }

        tinyMceService.configuration().then(function(config){
            $scope.tinyMceConfig = config;
        });
            
        stylesheetResource.getAll().then(function(stylesheets){
            $scope.stylesheets = stylesheets;
        });

        $scope.selected = function(alias, lookup){
            return lookup.indexOf(alias) >= 0;
        };

        $scope.selectCommand = function(command){
            var index = $scope.model.value.toolbar.indexOf(command.frontEndCommand);

            if(command.selected && index === -1){
                $scope.model.value.toolbar.push(command.frontEndCommand);
            }else if(index >= 0){
                $scope.model.value.toolbar.splice(index, 1);
            }
        };

        $scope.selectStylesheet = function(css){
            var index = $scope.model.value.stylesheets.indexOf(css.path);

            if(css.selected && index === -1){
                $scope.model.value.stylesheets.push(css.path);
            }else if(index >= 0){
                $scope.model.value.stylesheets.splice(index, 1);
            }
        };
    });
