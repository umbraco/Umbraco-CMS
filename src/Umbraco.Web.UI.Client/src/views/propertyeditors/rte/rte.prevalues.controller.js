angular.module("umbraco").controller("Umbraco.PrevalueEditors.RteController",
    function ($scope, $timeout, $log, tinyMceService, stylesheetResource) {
        var cfg = tinyMceService.defaultPrevalues();

        if($scope.model.value){
            if(angular.isString($scope.model.value)){
                $scope.model.value = cfg;
            }
        }else{
            $scope.model.value = cfg;
        }

        if (!$scope.model.value.stylesheets) {
            $scope.model.value.stylesheets = [];
        }
        if (!$scope.model.value.toolbar) {
            $scope.model.value.toolbar = [];
        }
        if (!$scope.model.value.maxImageSize && $scope.model.value.maxImageSize != 0) {
            $scope.model.value.maxImageSize = cfg.maxImageSize;
        }

        tinyMceService.configuration().then(function(config){
            $scope.tinyMceConfig = config;

        });

        stylesheetResource.getAll().then(function(stylesheets){
            $scope.stylesheets = stylesheets;
        });

        $scope.selected = function(cmd, alias, lookup){
            if (lookup && angular.isArray(lookup)) {
                cmd.selected = lookup.indexOf(alias) >= 0;
                return cmd.selected;
            }
            return false;
        };

        $scope.selectCommand = function(command){
            var index = $scope.model.value.toolbar.indexOf(command.frontEndCommand);

            if(command.selected && index === -1){
                $scope.model.value.toolbar.push(command.frontEndCommand);
            }else if(index >= 0){
                $scope.model.value.toolbar.splice(index, 1);
            }
        };

        $scope.selectStylesheet = function (css) {
            
            var index = $scope.model.value.stylesheets.indexOf(css.name);

            if(css.selected && index === -1){
                $scope.model.value.stylesheets.push(css.name);
            }else if(index >= 0){
                $scope.model.value.stylesheets.splice(index, 1);
            }
        };

        var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {

            var commands = _.where($scope.tinyMceConfig.commands, {selected: true});
            $scope.model.value.toolbar = _.pluck(commands, "frontEndCommand");
            
        });

        //when the scope is destroyed we need to unsubscribe
        $scope.$on('$destroy', function () {
            unsubscribe();
        });

    });
