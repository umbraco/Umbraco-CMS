angular.module("umbraco")
    .controller("Umbraco.Editors.ListViewController", 
        function ($rootScope, $scope, $routeParams, contentResource, contentTypeResource) {
        
        $scope.options = {
            pageSize: 10,
            pageNumber: 1,
            filter: '',
            orderBy: 'id',
            orderDirection: "desc"
        };

        
        $scope.next = function(){
            if ($scope.options.pageNumber < $scope.listViewResultSet.totalPages) {
                $scope.options.pageNumber++;
                $scope.reloadView();   
            }
        };

        $scope.goToPage = function (pageNumber) {
            $scope.options.pageNumber = pageNumber + 1;
            $scope.reloadView();
        };

        $scope.sort = function(field){
            $scope.options.sortby = field;
            
            if(field !== $scope.options.sortby){
                if($scope.options.order === "desc"){
                    $scope.options.order = "asc";
                }else{
                    $scope.options.order = "desc";
                }
            }
            $scope.reloadView();
        };

        $scope.prev = function(){
            if ($scope.options.pageNumber > 1) {
                $scope.options.pageNumber--;
                $scope.reloadView();
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


        if($routeParams.id){
            $scope.pagination = new Array(100);
            $scope.listViewAllowedTypes = contentTypeResource.getAllowedTypes($routeParams.id);
            $scope.reloadView($routeParams.id);
        }
        
});
