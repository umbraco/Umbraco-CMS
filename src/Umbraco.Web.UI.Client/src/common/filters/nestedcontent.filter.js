// Filter to take a node id and grab it's name instead
// Usage: {{ pickerAlias | ncNodeName }}

// Cache for node names so we don't make a ton of requests
var ncNodeNameCache = {
    id: "",
    keys: {}
};

angular.module("umbraco.filters").filter("ncNodeName", function (editorState, entityResource, $q) {

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
            return ncNodeNameCache.keys[lookupId];
        }

        // No value, so go fetch one 
        // We'll put a temp value in the cache though so we don't 
        // make a load of requests while we wait for a response
        ncNodeNameCache.keys[lookupId] = "Loading...";

        entityResource.getById(lookupId, lookupId.indexOf("umb://media/") === 0 ? "Media" : "Document")
            .then(
                function (ent) {
                    // If there is more than one item selected, append ", ..." to the header to hint that
                    ncNodeNameCache.keys[lookupId] = ent.name + (ids.length > 1 ? ", ..." : "");
                }
            );

        // Return the current value for now
        return ncNodeNameCache.keys[lookupId];
    };

}).filter("ncRichtext", function () {
    return function(input) {
        return $("<div/>").html(input).text();
    };
});
