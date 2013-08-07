angular.module("umbraco")
    .controller("Umbraco.Editors.ListViewController", 
        function ($rootScope, $scope, $routeParams, contentResource, contentTypeResource) {
        
        $scope.options = {
            take: 10,
            offset: 0,
            filter: '',
            sortby: 'id',
            order: "desc"
        };

        
        $scope.next = function(){
            if($scope.options.offset < $scope.listViewResultSet.pages){
                $scope.options.offset++;
                $scope.reloadView();    
            }
        };

        $scope.goToOffset = function(offset){
            $scope.options.offset = offset;
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
            if($scope.options.offset > 0){
                $scope.options.offset--;                
                $scope.reloadView();
            }
        };

        /*Loads the search results, based on parameters set in prev,next,sort and so on*/
        /*Pagination is done by an array of objects, due angularJS's funky way of monitoring state
        with simple values */
        $scope.reloadView = function(id){
                $scope.listViewResultSet = contentResource.getChildren(id, $scope.options);
                
                $scope.pagination = [];
                for (var i = $scope.listViewResultSet.pages - 1; i >= 0; i--) {
                        $scope.pagination[i] = {index: i, name: i+1};
                };
                
                if($scope.options.offset > $scope.listViewResultSet.pages){
                    $scope.options.offset = $scope.listViewResultSet.pages;
                }       
        };


        if($routeParams.id){
            $scope.pagination = new Array(100);
            $scope.listViewAllowedTypes = contentTypeResource.getAllowedTypes($routeParams.id);
            $scope.reloadView($routeParams.id);  
        }
        
});