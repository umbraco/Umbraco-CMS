//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
function mediaPickerController($scope, dialogService, entityResource, $log, iconHelper) {

    function trim(str, chr) {
        var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^' + chr + '+|' + chr + '+$', 'g');
        return str.replace(rgxtrim, '');
    }

    $scope.renderModel = [];   

    var dialogOptions = {
        multiPicker: false,
        entityType: "Media",
        section: "media",
        treeAlias: "media",
        callback: function(data) {
            if (angular.isArray(data)) {
                _.each(data, function (item, i) {
                    $scope.add(item);
                });
            }
            else {
                $scope.clear();
                $scope.add(data);
            }
        }
    };

    $scope.openContentPicker = function(){
        var d = dialogService.treePicker(dialogOptions);
    };

    $scope.remove =function(index){
        $scope.renderModel.splice(index, 1);
    };

    $scope.clear = function() {
        $scope.renderModel = [];
    };

    $scope.add = function (item) {
        var currIds = _.map($scope.renderModel, function (i) {
            return i.id;
        });
        if (currIds.indexOf(item.id) < 0) {
            item.icon = iconHelper.convertFromLegacyIcon(item.icon);
            $scope.renderModel.push({name: item.name, id: item.id, icon: item.icon});
        }	
    };

    var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
        var currIds = _.map($scope.renderModel, function (i) {
            return i.id;
        });
        $scope.model.value = trim(currIds.join(), ",");
    });

    //when the scope is destroyed we need to unsubscribe
    $scope.$on('$destroy', function () {
        unsubscribe();
    });

    //load media data
    var modelIds = $scope.model.value ? $scope.model.value.split(',') : [];
    entityResource.getByIds(modelIds, dialogOptions.entityType).then(function (data) {
        _.each(data, function (item, i) {
            item.icon = iconHelper.convertFromLegacyIcon(item.icon);
            $scope.renderModel.push({ name: item.name, id: item.id, icon: item.icon });
        });
    });
    
}

angular.module('umbraco').controller("Umbraco.PrevalueEditors.MediaPickerController",mediaPickerController);