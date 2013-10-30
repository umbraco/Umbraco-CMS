//used for the member picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.MemberGroupPickerController",
    function($scope, eventsService, entityResource, searchService, $log) {
        var dialogOptions = $scope.$parent.dialogOptions;
        $scope.dialogTreeEventHandler = $({});
        $scope.results = [];

        /** Method used for selecting a node */
        function select(text, id, entity) {

           
            $scope.showSearch = false;
            $scope.results = [];
            $scope.term = "";
            $scope.oldTerm = undefined;

            if (dialogOptions.multiPicker) {
                $scope.select(id);
            }
            else {
                $scope.submit(id);
               
            }
        }


        $scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args) {
            args.event.preventDefault();
            args.event.stopPropagation();


            eventsService.publish("Umbraco.Dialogs.MemberGroupPickerController.Select", args).then(function(a) {

                //This is a tree node, so we don't have an entity to pass in, it will need to be looked up
                //from the server in this method.
                select(a.node.name, a.node.id);

                if (dialogOptions && dialogOptions.multipicker) {

                    var c = $(a.event.target.parentElement);
                    if (!a.node.selected) {
                        a.node.selected = true;
                        c.find("i.umb-tree-icon").hide()
                            .after("<i class='icon umb-tree-icon sprTree icon-check blue temporary'></i>");
                    }
                    else {
                        a.node.selected = false;
                        c.find(".temporary").remove();
                        c.find("i.umb-tree-icon").show();
                    }
                }
            });

        });
    });