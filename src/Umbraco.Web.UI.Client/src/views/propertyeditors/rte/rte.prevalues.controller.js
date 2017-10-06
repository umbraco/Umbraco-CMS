angular.module("umbraco").controller("Umbraco.PrevalueEditors.RteController",
    function ($scope, $timeout, $log, tinyMceService, stylesheetResource, assetsService) {
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

            // extend commands with properties for font-icon and if it is a custom command
            $scope.tinyMceConfig.commands = _.map($scope.tinyMceConfig.commands, function (obj) {
                var icon = getFontIcon(obj.frontEndCommand);
                return angular.extend(obj, {
                    fontIcon: icon.name,
                    isCustom: icon.isCustom
                });
            });
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
        
        // map properties for specific commands
        function getFontIcon(alias) {
            var icon = { name: alias, isCustom: false };

            switch (alias) {
                case "codemirror":
                    icon.name = "code";
                    icon.isCustom = false;
                    break;
                case "styleselect":
                case "fontsizeselect":
                    icon.name = "icon-list";
                    icon.isCustom = true;
                    break;
                case "umbembeddialog":
                    icon.name = "icon-tv";
                    icon.isCustom = true;
                    break;
                case "umbmediapicker":
                    icon.name = "icon-picture";
                    icon.isCustom = true;
                    break;
                case "umbmacro":
                    icon.name = "icon-settings-alt";
                    icon.isCustom = true;
                    break;
                case "umbmacro":
                    icon.name = "icon-settings-alt";
                    icon.isCustom = true;
                    break;
                default:
                    icon.name = alias;
                    icon.isCustom = false;
            }

            return icon;
        }

        var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {

            var commands = _.where($scope.tinyMceConfig.commands, {selected: true});
            $scope.model.value.toolbar = _.pluck(commands, "frontEndCommand");
            
        });

        // when the scope is destroyed we need to unsubscribe
        $scope.$on('$destroy', function () {
            unsubscribe();
        });

        // load TinyMCE skin which contains css for font-icons
        assetsService.loadCss("lib/tinymce/skins/umbraco/skin.min.css");
    });