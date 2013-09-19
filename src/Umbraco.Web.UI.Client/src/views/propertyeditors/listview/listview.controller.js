angular.module("umbraco")
    .controller("Umbraco.Editors.ListViewController", 
        function ($rootScope, $scope, $routeParams, contentResource, contentTypeResource, editorContextService) {

        $scope.selected = [];
            
        $scope.options = {
            pageSize: 10,
            pageNumber: 1,
            filter: '',
            orderBy: 'Id',
            orderDirection: "desc"
        };

        
        $scope.next = function(){
            if ($scope.options.pageNumber < $scope.listViewResultSet.totalPages) {
                $scope.options.pageNumber++;
                $scope.reloadView($scope.content.id);
            }
        };

        $scope.goToPage = function (pageNumber) {
            $scope.options.pageNumber = pageNumber + 1;
            $scope.reloadView($scope.content.id);
        };

        $scope.sort = function (field) {
        
            $scope.options.orderBy = field;
            
          
            if ($scope.options.orderDirection === "desc") {
                $scope.options.orderDirection = "asc";
            }else{
                $scope.options.orderDirection = "desc";
            }
            
           
            $scope.reloadView($scope.content.id);
        };

        $scope.prev = function(){
            if ($scope.options.pageNumber > 1) {
                $scope.options.pageNumber--;
                $scope.reloadView($scope.content.id);
            }
        };

        /*Loads the search results, based on parameters set in prev,next,sort and so on*/
        /*Pagination is done by an array of objects, due angularJS's funky way of monitoring state
        with simple values */
                
        $scope.reloadView = function(id) {
            contentResource.getChildren(id, $scope.options).then(function(data) {
                
                $scope.listViewResultSet = data;
                $scope.pagination = [];

                for (var i = $scope.listViewResultSet.totalPages - 1; i >= 0; i--) {
                    $scope.pagination[i] = { index: i, name: i + 1 };
                }

                if ($scope.options.pageNumber > $scope.listViewResultSet.totalPages) {
                    $scope.options.pageNumber = $scope.listViewResultSet.totalPages;
                }

            });
        };

        var updateSelected = function (action, id) {
            if (action === 'add' && $scope.selected.indexOf(id) === -1) {
                $scope.selected.push(id);
            }
            if (action === 'remove' && $scope.selected.indexOf(id) !== -1) {
                $scope.selected.splice($scope.selected.indexOf(id), 1);
            }
        };

        $scope.updateSelection = function ($event, id) {
            var checkbox = $event.target;
            var action = (checkbox.checked ? 'add' : 'remove');
            updateSelected(action, id);
        };

        $scope.selectAll = function ($event) {
            var checkbox = $event.target;
            var action = (checkbox.checked ? 'add' : 'remove');
            for (var i = 0; i < $scope.listViewResultSet.items.length; i++) {
                var entity = $scope.listViewResultSet.items[i];
                updateSelected(action, entity.id);
            }
        };

        $scope.getSelectedClass = function (entity) {
            return $scope.isSelected(entity.id) ? 'selected' : '';
        };

        $scope.isSelected = function (id) {
            return $scope.selected.indexOf(id) >= 0;
        };

        $scope.isSelectedAll = function () {
            if ($scope.listViewResultSet != null)
                return $scope.selected.length === $scope.listViewResultSet.items.length;
            else
                return false;
        };

        $scope.isAnythingSelected = function() {
            return $scope.selected.length > 0;
        };

        $scope.delete = function () {
            $scope.bulkStatus = "Starting with delete";
            var current = 1;
            var total = $scope.selected.length;
            for (var i = 0; i < $scope.selected.length; i++) {
                contentResource.deleteById($scope.selected[i]).then(function (data) {
                    $scope.bulkStatus = "Deleted doc" + i + " out of "+ total +"documents";
                    if (current == total) {
                        $scope.bulkStatus = "Deleting done";
                        $scope.reloadView($scope.content.id);
                    }
                    current++;
                });
            }
            
        };

        $scope.publish = function () {
            $scope.bulkStatus = "Starting with publish";
            var current = 1;
            var total = $scope.selected.length;
            for (var i = 0; i < $scope.selected.length; i++) {
                contentResource.getById($scope.selected[i]).then(function(content) {
                    contentResource.publish(content, false)
                        .then(function(content){
                            $scope.bulkStatus = "Publishing doc" + i + " out of " + total + "documents";
                            if (current == total) {
                                $scope.bulkStatus = "Publish done";
                                $scope.reloadView($scope.content.id);
                            }
                            current++;
                        });
                });
            }
        };
 
        $scope.unpublish = function () {

        };
            
        if($routeParams.id){
            $scope.pagination = new Array(100);
            $scope.listViewAllowedTypes = contentTypeResource.getAllowedTypes($routeParams.id);
            $scope.reloadView($routeParams.id);

            $scope.content = editorContextService.getContext();
            
        }
        
});
