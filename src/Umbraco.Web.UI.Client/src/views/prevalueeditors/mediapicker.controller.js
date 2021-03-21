//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
function mediaPickerController($scope, entityResource, iconHelper, editorService, angularHelper) {

    function trim(str, chr) {
        var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^' + chr + '+|' + chr + '+$', 'g');
        return str.replace(rgxtrim, '');
    }

    $scope.renderModel = [];   

    $scope.allowRemove = true;
    $scope.allowEdit = true;
    $scope.sortable = false;

    var dialogOptions = {
        multiPicker: false,
        entityType: "Media",
        section: "media",
        treeAlias: "media",
        idType: "udi"
    };

    //combine the dialogOptions with any values returned from the server
    if ($scope.model.config) {
        Utilities.extend(dialogOptions, $scope.model.config);
    }

    $scope.openTreePicker = function () {
        var treePicker = dialogOptions;

        treePicker.submit = function (model) {
            if (treePicker.multiPicker) {
                _.each(model.selection, function (item, i) {
                    $scope.add(item);
                });
            } else {
                $scope.clear();
                $scope.add(model.selection[0]);
            }
            editorService.close();
        };

        treePicker.close = function () {
            editorService.close();
        };

        editorService.treePicker(treePicker);
    }

    $scope.remove =function(index){
        $scope.renderModel.splice(index, 1);
        syncModelValue();
    };

    $scope.clear = function() {
        $scope.renderModel = [];
        syncModelValue();
    };

    $scope.add = function (item) {

        var itemId = dialogOptions.idType === "udi" ? item.udi : item.id;

        var currIds = _.map($scope.renderModel, function (i) {
            return dialogOptions.idType === "udi" ? i.udi : i.id;
        });
        if (currIds.indexOf(itemId) < 0) {
                
            item.icon = iconHelper.convertFromLegacyIcon(item.icon);
            $scope.renderModel.push({ name: item.name, id: item.id, icon: item.icon, udi: item.udi });

            // store the index of the new item in the renderModel collection so we can find it again
            var itemRenderIndex = $scope.renderModel.length - 1;
			// get and update the path for the picked node
            entityResource.getUrl(item.id, dialogOptions.entityType).then(function(data){
			    $scope.renderModel[itemRenderIndex].path = data;
            });

        }	

        syncModelValue();
    };

    function syncModelValue() {
        var currIds = _.map($scope.renderModel, function (i) {
            return dialogOptions.idType === "udi" ? i.udi : i.id;
        });
        $scope.model.value = trim(currIds.join(), ",");
        angularHelper.getCurrentForm($scope).$setDirty();
    }

    //load media data
    var modelIds = $scope.model.value ? $scope.model.value.split(',') : [];
    if (modelIds.length > 0) {
        entityResource.getByIds(modelIds, dialogOptions.entityType).then(function (data) {
            _.each(data, function (item, i) {

                item.icon = iconHelper.convertFromLegacyIcon(item.icon);
                $scope.renderModel.push({ name: item.name, id: item.id,  icon: item.icon, udi: item.udi });
                
                // store the index of the new item in the renderModel collection so we can find it again
                var itemRenderIndex = $scope.renderModel.length - 1;
                // get and update the path for the picked node
                entityResource.getUrl(item.id, dialogOptions.entityType).then(function(data){
                    $scope.renderModel[itemRenderIndex].path = data;
                });

            });
        });
    }
        
}

angular.module('umbraco').controller("Umbraco.PrevalueEditors.MediaPickerController",mediaPickerController);
