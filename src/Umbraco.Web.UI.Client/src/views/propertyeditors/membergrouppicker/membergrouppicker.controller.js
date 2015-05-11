//this controller simply tells the dialogs service to open a memberPicker window
//with a specified callback, this callback will receive an object with a selection on it
function memberGroupPicker($scope, dialogService){

    function trim(str, chr) {
        var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^' + chr + '+|' + chr + '+$', 'g');
        return str.replace(rgxtrim, '');
    }

    $scope.renderModel = [];
    
    if ($scope.model.value) {
        var modelIds = $scope.model.value.split(',');
        _.each(modelIds, function (item, i) {
            $scope.renderModel.push({ name: item, id: item, icon: 'icon-users' });
        });
    }
	    
    var dialogOptions = {
        multiPicker: true,
        entityType: "MemberGroup",
        section: "membergroup",
        treeAlias: "memberGroup",
        filter: "",
        filterCssClass: "not-allowed",
        callback: function (data) {
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
    if($scope.model.config){
        angular.extend(dialogOptions, $scope.model.config);
    }

    $scope.openMemberGroupPicker =function() {
        var d = dialogService.memberGroupPicker(dialogOptions);
    };


    $scope.remove =function(index){
        $scope.renderModel.splice(index, 1);
    };

    $scope.add = function (item) {
        var currIds = _.map($scope.renderModel, function (i) {
            return i.id;
        });

        if (currIds.indexOf(item) < 0) {
            $scope.renderModel.push({ name: item, id: item, icon: 'icon-users' });
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

}

angular.module('umbraco').controller("Umbraco.PropertyEditors.MemberGroupPickerController", memberGroupPicker);