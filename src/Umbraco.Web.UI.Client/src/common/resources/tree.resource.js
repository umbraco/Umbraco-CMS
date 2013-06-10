/**
    * @ngdoc factory 
    * @name umbraco.resources.treeResource     
    * @description Loads in data for trees
    **/
function treeResource($q, $http) {

    /** internal method to get the tree app url */
    function getTreeAppUrl(section) {
        return Umbraco.Sys.ServerVariables.treeApplicationApiBaseUrl + "GetApplicationTrees?application=" + section;
    }
    /** internal method to get the tree node's children url */
    function getTreeNodesUrl(node) {
        if (!node.childNodesUrl){
            throw "No childNodesUrl property found on the tree node, cannot load child nodes";
        }
        return node.childNodesUrl;
    }

    //the factory object returned
    return {
        /** Loads in the data to display the nodes for an application */
        loadApplication: function (section) {

            var deferred = $q.defer();

            //go and get the tree data
            $http.get(getTreeAppUrl(section)).
                success(function (data, status, headers, config) {
                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for application tree ' + section);
                });

            return deferred.promise;
        },
        /** Loads in the data to display the child nodes for a given node */
        loadNodes: function (section, node) {

            var deferred = $q.defer();

            //go and get the tree data
            $http.get(getTreeNodesUrl(node)).
                success(function (data, status, headers, config) {
                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    deferred.reject('Failed to retreive data for child nodes ' + node.nodeId);
                });

            return deferred.promise;

        }
    };
}

angular.module('umbraco.resources').factory('treeResource', treeResource);