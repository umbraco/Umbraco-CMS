angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.TextStringController",
    function ($scope, $rootScope, $timeout, dialogService) {

        $scope.adjustSize = function(ev){
            if(ev.target.scrollHeight > ev.target.clientHeight){
                $(ev.target).height(ev.target.scrollHeight);
            }
        };

        if($scope.control.value === null){
            $timeout(function(){
                $("#" + $scope.control.uniqueId +"_text").focus();
            }, 200);
        }

    });

