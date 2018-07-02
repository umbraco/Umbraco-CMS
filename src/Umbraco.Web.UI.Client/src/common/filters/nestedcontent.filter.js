// Filter to take a node id and grab it's name instead
// Usage: {{ pickerAlias | ncNodeName }}

// Cache for node names so we don't make a ton of requests
var ncNodeNameCache = {
    id: "",
    keys: {}
};

angular.module("umbraco.filters").filter("ncNodeName", function (editorState, entityResource) {

    function formatLabel(firstNodeName, totalNodes) {
        return totalNodes <= 1
            ? firstNodeName
            // If there is more than one item selected, append the additional number of items selected to hint that
            : firstNodeName + " (+" + (totalNodes - 1) + ")";
    }

    return function (input) {

        // Check we have a value at all
        if (input === "" || input.toString() === "0") {
            return "";
        }

        var currentNode = editorState.getCurrent();

        // Ensure a unique cache per editor instance
        var key = "ncNodeName_" + currentNode.key;
        if (ncNodeNameCache.id !== key) {
            ncNodeNameCache.id = key;
            ncNodeNameCache.keys = {};
        }

        // MNTP values are comma separated IDs. We'll only fetch the first one for the NC header.
        var ids = input.split(',');
        var lookupId = ids[0];

        // See if there is a value in the cache and use that
        if (ncNodeNameCache.keys[lookupId]) {
            return formatLabel(ncNodeNameCache.keys[lookupId], ids.length);
        }

        // No value, so go fetch one 
        // We'll put a temp value in the cache though so we don't 
        // make a load of requests while we wait for a response
        ncNodeNameCache.keys[lookupId] = "Loading...";

        var type = lookupId.indexOf("umb://media/") === 0
            ? "Media"
            : lookupId.indexOf("umb://member/") === 0
                ? "Member"
                : "Document";
        entityResource.getById(lookupId, type)
            .then(
                function (ent) {
                    ncNodeNameCache.keys[lookupId] = ent.name;
                }
            );

        // Return the current value for now
        return formatLabel(ncNodeNameCache.keys[lookupId], ids.length);
    };

}).filter("ncRichText", function () {
    return function(input) {
        return $("<div/>").html(input).text();
    };
});
