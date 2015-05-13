//this controller simply tells the dialogs service to open a memberPicker window
//with a specified callback, this callback will receive an object with a selection on it
function memberPickerController($scope, dialogService, entityResource, $log, iconHelper){

    function trim(str, chr) {
        var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^' + chr + '+|' + chr + '+$', 'g');
        return str.replace(rgxtrim, '');
    }

    $scope.renderModel = [];

    var dialogOptions = {
        multiPicker: false,
        entityType: "Member",
        section: "member",
        treeAlias: "member",
        filter: function(i) {
            return i.metaData.isContainer == true;
        },
        filterCssClass: "not-allowed",
        callback: function(data) {
            if (angular.isArray(data)) {
                _.each(data, function (item, i) {
                    $scope.add(item);
                });
            } else {
                $scope.clear();
                $scope.add(data);
            }
        }
    };

    //since most of the pre-value config's are used in the dialog options (i.e. maxNumber, minNumber, etc...) we'll merge the 
    // pre-value config on to the dialog options
    if ($scope.model.config) {
        angular.extend(dialogOptions, $scope.model.config);
    }
    
    $scope.openMemberPicker =function() {
        var d = dialogService.memberPicker(dialogOptions);
    };


    $scope.remove =function(index){
        $scope.renderModel.splice(index, 1);
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

    $scope.clear = function() {
        $scope.renderModel = [];
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

    //load member data
    var modelIds = $scope.model.value ? $scope.model.value.split(',') : [];
    entityResource.getByIds(modelIds, "Member").then(function (data) {
        _.each(data, function (item, i) {
            item.icon = iconHelper.convertFromLegacyIcon(item.icon);
            $scope.renderModel.push({ name: item.name, id: item.id, icon: item.icon });
        });
    });
}


angular.module('umbraco').controller("Umbraco.PropertyEditors.MemberPickerController", memberPickerController);