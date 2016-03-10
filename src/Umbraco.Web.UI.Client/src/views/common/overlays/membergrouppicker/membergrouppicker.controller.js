//used for the member picker dialog
angular.module("umbraco").controller("Umbraco.Overlays.MemberGroupPickerController",
    function($scope, eventsService, entityResource, searchService, $log, localizationService) {

        if(!$scope.model.title) {
            $scope.model.title = localizationService.localize("defaultdialogs_selectMemberGroup");
        }

        $scope.dialogTreeEventHandler = $({});
        $scope.multiPicker = $scope.model.multiPicker;

        function activate() {

          if($scope.multiPicker) {
             $scope.model.selectedMemberGroups = [];
          } else {
             $scope.model.selectedMemberGroup = "";
          }

        }

        function selectMemberGroup(id) {
           $scope.model.selectedMemberGroup = id;
        }

        function selectMemberGroups(id) {
           $scope.model.selectedMemberGroups.push(id);
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

        function nodeSelectHandler(ev, args) {
            args.event.preventDefault();
            args.event.stopPropagation();

            eventsService.emit("dialogs.memberGroupPicker.select", args);

            //This is a tree node, so we don't have an entity to pass in, it will need to be looked up
            //from the server in this method.
            select(args.node.name, args.node.id);

            //toggle checked state
            args.node.selected = args.node.selected === true ? false : true;
        }

        $scope.dialogTreeEventHandler.bind("treeNodeSelect", nodeSelectHandler);

        $scope.$on('$destroy', function () {
            $scope.dialogTreeEventHandler.unbind("treeNodeSelect", nodeSelectHandler);
        });

        activate();

    });
