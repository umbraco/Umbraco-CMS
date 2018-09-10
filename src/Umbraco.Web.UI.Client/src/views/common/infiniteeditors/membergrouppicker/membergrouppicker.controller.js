//used for the member picker dialog
angular.module("umbraco").controller("Umbraco.Editors.MemberGroupPickerController",
    function($scope, eventsService, localizationService) {

        var vm = this;

        $scope.dialogTreeApi = {};
        $scope.multiPicker = $scope.model.multiPicker;

        vm.submit = submit;
        vm.close = close;

        function activate() {

            if(!$scope.model.title) {
                localizationService.localize("defaultdialogs_selectMemberGroup").then(function(value){
                    $scope.model.title = value;
                });
            }

            if ($scope.multiPicker) {
                $scope.model.selectedMemberGroups = [];
            } else {
                $scope.model.selectedMemberGroup = "";
            }

        }

        function selectMemberGroup(id) {
           $scope.model.selectedMemberGroup = id;
        }

        function selectMemberGroups(id) {
            var index = $scope.model.selectedMemberGroups.indexOf(id);

            if(index === -1){
                // If the id does not exists in the array then add it
                $scope.model.selectedMemberGroups.push(id);
            }
            else{
                // Otherwise we will remove it from the array instead
                $scope.model.selectedMemberGroups.splice(index, 1);
            }
        }

        /** Method used for selecting a node */
        function select(text, id) {

            if ($scope.model.multiPicker) {
               selectMemberGroups(id);
            }
            else {
               selectMemberGroup(id);
               $scope.model.submit($scope.model);
            }
        }

        function nodeSelectHandler(args) {
            args.event.preventDefault();
            args.event.stopPropagation();

            eventsService.emit("dialogs.memberGroupPicker.select", args);

            //This is a tree node, so we don't have an entity to pass in, it will need to be looked up
            //from the server in this method.
            select(args.node.name, args.node.id);

            //toggle checked state
            args.node.selected = args.node.selected === true ? false : true;
        }

        $scope.onTreeInit = function () {
            $scope.dialogTreeApi.callbacks.treeNodeSelect(nodeSelectHandler);
        };
        
        function close() {
            if($scope.model && $scope.model.close) {
                $scope.model.close();
            }
        }
    
        function submit() {
            if($scope.model && $scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        }
        
        activate();

    });
