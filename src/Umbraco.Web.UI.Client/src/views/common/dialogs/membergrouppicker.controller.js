//used for the member picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.MemberGroupPickerController",
    function($scope, eventsService, entityResource, searchService, $log) {
        var dialogOptions = $scope.dialogOptions;
        $scope.dialogTreeEventHandler = $({});
        $scope.multiPicker = dialogOptions.multiPicker;

        /** Method used for selecting a node */
        function select(text, id) {

            if (dialogOptions.multiPicker) {
                $scope.select(id);              
            }
            else {
                $scope.submit(id);               
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
    });