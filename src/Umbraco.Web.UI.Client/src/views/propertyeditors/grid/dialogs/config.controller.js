angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.GridController.Dialogs.Config",
    function ($scope, $http) {

        var placeHolder = "{0}";
        var addModifier = function(val, modifier){
            if (!modifier || modifier.indexOf(placeHolder) < 0) {
                return val;
            } else {
                return modifier.replace(placeHolder, val);
            }
        }

        var stripModifier = function (val, modifier) {
            if (!val || !modifier || modifier.indexOf(placeHolder) < 0) {
                return val;
            } else {
                var paddArray = modifier.split(placeHolder);
                if(paddArray.length == 1){
                    if (modifier.indexOf(placeHolder) === 0) {
                        return val.slice(0, -paddArray[0].length);
                    } else {
                        return val.slice(paddArray[0].length, 0);
                    }
                } else {
                    if (paddArray[1].length === 0) {
                        return val.slice(paddArray[0].length);
                    }
                    return val.slice(paddArray[0].length, -paddArray[1].length); 
                }
            }
        }


        $scope.styles = _.filter( angular.copy($scope.dialogOptions.config.items.styles), function(item){return (item.applyTo === undefined || item.applyTo === $scope.dialogOptions.itemType); });
        $scope.config = _.filter( angular.copy($scope.dialogOptions.config.items.config), function(item){return (item.applyTo === undefined || item.applyTo === $scope.dialogOptions.itemType); });


        var element = $scope.dialogOptions.gridItem;
        if(angular.isObject(element.config)){
            _.each($scope.config, function(cfg){
                var val = element.config[cfg.key];
                if(val){
                    cfg.value = stripModifier(val, cfg.modifier);
                }
            });
        }

        if(angular.isObject(element.styles)){
            _.each($scope.styles, function(style){
                var val = element.styles[style.key];
                if(val){
                    style.value = stripModifier(val, style.modifier);
                }
            });
        }


        $scope.saveAndClose = function(){
            var styleObject = {};
            var configObject = {};

            _.each($scope.styles, function(style){
                if(style.value){
                    styleObject[style.key] = addModifier(style.value, style.modifier);
                }
            });
            _.each($scope.config, function (cfg) {
                if (cfg.value) {
                    configObject[cfg.key] = addModifier(cfg.value, cfg.modifier);
                }
            });

            $scope.submit({config: configObject, styles: styleObject});
        };

    });
