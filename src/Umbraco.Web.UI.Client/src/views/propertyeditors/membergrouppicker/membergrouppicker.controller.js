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

    $scope.openMemberGroupPicker = function() {

      $scope.memberGroupPicker = {};
      $scope.memberGroupPicker.multiPicker = true;
      $scope.memberGroupPicker.view = "memberGroupPicker";
      $scope.memberGroupPicker.show = true;

      $scope.memberGroupPicker.submit = function(model) {

         if(model.selectedMemberGroups) {
            _.each(model.selectedMemberGroups, function (item, i) {
                $scope.add(item);
            });
         }

         if(model.selectedMemberGroup) {
            $scope.clear();
            $scope.add(model.selectedMemberGroup);
         }

         $scope.memberGroupPicker.show = false;
         $scope.memberGroupPicker = null;
      };

      $scope.memberGroupPicker.close = function(oldModel) {
         $scope.memberGroupPicker.show = false;
         $scope.memberGroupPicker = null;
      };

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
