angular.module("umbraco").controller("Umbraco.PrevalueEditors.RteController",
    function ($scope, $timeout, $log, tinyMceService, stylesheetResource, assetsService) {
        var cfg = tinyMceService.defaultPrevalues();

        if($scope.model.value){
            if(Utilities.isString($scope.model.value)){
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
        if (!$scope.model.value.mode) {
            $scope.model.value.mode = "classic";
        }

        tinyMceService.configuration().then(function(config){
            $scope.tinyMceConfig = config;

            // extend commands with properties for font-icon and if it is a custom command
            $scope.tinyMceConfig.commands = _.map($scope.tinyMceConfig.commands, function (obj) {
                var icon = getFontIcon(obj.alias);

                var objCmd = Utilities.extend(obj, {
                    fontIcon: icon.name,
                    isCustom: icon.isCustom,
                    selected: $scope.model.value.toolbar.indexOf(obj.alias) >= 0,
                    icon: "mce-ico " + (icon.isCustom ? ' mce-i-custom ' : ' mce-i-') + icon.name
                });

                return objCmd;
            });
        });

        stylesheetResource.getAll().then(function(stylesheets){
            $scope.stylesheets = stylesheets;
            
            // if the CSS directory changes, previously assigned stylesheets are retained, but will not be visible
            // and will throw a 404 when loading the RTE. Remove them here. Still needs to be saved...
            let cssPath = Umbraco.Sys.ServerVariables.umbracoSettings.cssPath;
            $scope.model.value.stylesheets = $scope.model.value.stylesheets
                .filter(sheet => sheet.startsWith(cssPath));
            
            $scope.stylesheets.forEach(stylesheet => {
                // support both current format (full stylesheet path) and legacy format (stylesheet name only) 
                stylesheet.selected = $scope.model.value.stylesheets.indexOf(stylesheet.path) >= 0 ||$scope.model.value.stylesheets.indexOf(stylesheet.name) >= 0;
            });
        });

        $scope.selectCommand = function(command){
            var index = $scope.model.value.toolbar.indexOf(command.alias);

            if(command.selected && index === -1){
                $scope.model.value.toolbar.push(command.alias);
            }else if(index >= 0){
                $scope.model.value.toolbar.splice(index, 1);
            }
        };

        $scope.selectStylesheet = function (css) {

            // find out if the stylesheet is already selected; first look for the full stylesheet path (current format)
            var index = $scope.model.value.stylesheets.indexOf(css.path);
            if (index === -1) {
                // ... then look for the stylesheet name (legacy format)
                index = $scope.model.value.stylesheets.indexOf(css.name);
            }

            if(index === -1){
                $scope.model.value.stylesheets.push(css.path);
            }else{
                $scope.model.value.stylesheets.splice(index, 1);
            }
        };

        // map properties for specific commands
        function getFontIcon(alias) {
            var icon = { name: alias, isCustom: false };

            switch (alias) {
                case "ace":
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
            $scope.model.value.toolbar = _.pluck(commands, "alias");

        });

        // when the scope is destroyed we need to unsubscribe
        $scope.$on('$destroy', function () {
            unsubscribe();
        });

        // load TinyMCE skin which contains css for font-icons
        assetsService.loadCss("lib/tinymce/skins/lightgray/skin.min.css", $scope);
    });
