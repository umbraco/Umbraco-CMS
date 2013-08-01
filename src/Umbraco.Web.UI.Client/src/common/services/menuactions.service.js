
function umbracoMenuActions($q, treeService) {
    
    return {
        refresh: function(args) {
            treeService.loadNodeChildren({ node: args.treeNode, section: args.section });
        }
    };
} 

angular.module('umbraco.services').factory('umbracoMenuActions', umbracoMenuActions);