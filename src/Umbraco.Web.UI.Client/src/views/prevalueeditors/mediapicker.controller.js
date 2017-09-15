//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
function mediaPickerController($scope, dialogService, entityResource, $log, iconHelper) {

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
        idType: "int"
    };

    //combine the dialogOptions with any values returned from the server
    if ($scope.model.config) {
        angular.extend(dialogOptions, $scope.model.config);
    }

    $scope.openContentPicker = function() {
      $scope.contentPickerOverlay = dialogOptions;
      $scope.contentPickerOverlay.view = "treePicker";
      $scope.contentPickerOverlay.show = true;

      $scope.contentPickerOverlay.submit = function(model) {

         if ($scope.contentPickerOverlay.multiPicker) {
             _.each(model.selection, function (item, i) {
                 $scope.add(item);
             });
         }
         else {
             $scope.clear();
             $scope.add(model.selection[0]);
         }

         $scope.contentPickerOverlay.show = false;
         $scope.contentPickerOverlay = null;
      };

      $scope.contentPickerOverlay.close = function(oldModel) {
         $scope.contentPickerOverlay.show = false;
         $scope.contentPickerOverlay = null;
      };
    }

    $scope.remove =function(index){
        $scope.renderModel.splice(index, 1);
    };

    $scope.clear = function() {
        $scope.renderModel = [];
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
    };

    var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
        var currIds = _.map($scope.renderModel, function (i) {
            return dialogOptions.idType === "udi" ? i.udi : i.id;
        });
        $scope.model.value = trim(currIds.join(), ",");
    });

    //when the scope is destroyed we need to unsubscribe
    $scope.$on('$destroy', function () {
        unsubscribe();
    });

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