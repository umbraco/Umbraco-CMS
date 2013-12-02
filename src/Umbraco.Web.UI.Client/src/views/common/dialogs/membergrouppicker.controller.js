//used for the member picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.MemberGroupPickerController",
    function($scope, eventsService, entityResource, searchService, $log) {
        var dialogOptions = $scope.$parent.dialogOptions;
        $scope.dialogTreeEventHandler = $({});
        $scope.results = [];
        $scope.dialogData = [];
        
        /** Method used for selecting a node */
        function select(text, id) {

           
            $scope.showSearch = false;
            $scope.results = [];
            $scope.term = "";
            $scope.oldTerm = undefined;

            if (dialogOptions.multiPicker) {
                if ($scope.dialogData.indexOf(id) == -1) {
                    $scope.dialogData.push(id);
                }
            }
            else {
                $scope.submit(id);
               
            }
        }
        
        function remove(text, id) {
            var index = $scope.dialogData.indexOf(id);
         
            if (index > -1) {
                $scope.dialogData.splice(index, 1);
            }
        }


        $scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args) {
            args.event.preventDefault();
            args.event.stopPropagation();


            eventsService.emit("dialogs.memberGroupPicker.select", args);
            
            //This is a tree node, so we don't have an entity to pass in, it will need to be looked up
            //from the server in this method.
            select(args.node.name, args.node.id);

            if (dialogOptions.multiPicker) {
                var c = $(args.event.target.parentElement);
                if (!args.node.selected) {
                    args.node.selected = true;
                    c.find("i.umb-tree-icon").hide()
                        .after("<i class='icon umb-tree-icon sprTree icon-check blue temporary'></i>");
                }
                else {

                    remove(args.node.name, args.node.id);

                    args.node.selected = false;
                    c.find(".temporary").remove();
                    c.find("i.umb-tree-icon").show();
                }
            }

        });
    });