/**
    * @ngdoc service
    * @name umbraco.resources.treeResource
    * @description Loads in data for trees
    **/
function treeResource($q, $http, umbRequestHelper) {

    /** internal method to get the tree node's children url */
    function getTreeNodesUrl(node) {
        if (!node.childNodesUrl) {
            throw "No childNodesUrl property found on the tree node, cannot load child nodes";
        }
        return node.childNodesUrl;
    }

    /** internal method to get the tree menu url */
    function getTreeMenuUrl(node) {
        if (!node.menuUrl) {
            return null;
        }
        return node.menuUrl;
    }

    //the factory object returned
    return {

        /** Loads in the data to display the nodes menu */
        loadMenu: function (node) {
            var treeMenuUrl = getTreeMenuUrl(node);
            if (treeMenuUrl !== undefined && treeMenuUrl !== null && treeMenuUrl.length > 0) {
                return umbRequestHelper.resourcePromise(
                    $http.get(getTreeMenuUrl(node)),
                    "Failed to retrieve data for a node's menu " + node.id);
            } else {
                return $q.reject({
                    errorMsg: "No tree menu url defined for node " + node.id
                });
            }
        },

        /** Loads in the data to display the nodes for an application */
        loadApplication: function (options) {

            if (!options || !options.section) {
                throw "The object specified for does not contain a 'section' property";
            }

            if(!options.tree){
                options.tree = "";
            }
            if (!options.isDialog) {
                options.isDialog = false;
            }

            //create the query string for the tree request, these are the mandatory options:
            var query = "application=" + options.section
                        + "&tree=" + options.tree
                        + "&use=" + (options.isDialog ? "dialog" : "main");

            //the options can contain extra query string parameters
            if (options.queryString) {
                query += "&" + options.queryString;
            }

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "treeApplicationApiBaseUrl",
                        "GetApplicationTrees",
                            query)),
                'Failed to retrieve data for application tree ' + options.section);
        },

        /** Loads in the data to display the child nodes for a given node */
        loadNodes: function (options) {

            if (!options || !options.node) {
                throw "The options parameter object does not contain the required properties: 'node'";
            }

            return umbRequestHelper.resourcePromise(
                $http.get(getTreeNodesUrl(options.node)),
                'Failed to retrieve data for child nodes ' + options.node.nodeId);
        }
    };
}

angular.module('umbraco.resources').factory('treeResource', treeResource);
