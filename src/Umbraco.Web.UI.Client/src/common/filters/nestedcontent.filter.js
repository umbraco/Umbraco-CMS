// Filter to take a node id and grab it's name instead
// Usage: {{ pickerAlias | ncNodeName }}

// Cache for node names so we don't make a ton of requests
var ncNodeNameCache = {
  id: "",
  keys: {}
};

angular.module("umbraco.filters").filter("ncNodeName", function (editorState, entityResource, udiParser) {

  function formatLabel(firstNodeName, totalNodes) {
    return totalNodes <= 1
      ? firstNodeName
      // If there is more than one item selected, append the additional number of items selected to hint that
      : firstNodeName + " (+" + (totalNodes - 1) + ")";
  }

  nodeNameFilter.$stateful = true;
  function nodeNameFilter(input) {

    // Check we have a value at all
    if (typeof input === 'undefined' || input === "" || input.toString() === "0" || input === null) {
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
    var serviceInvoked = false;

    // See if there is a value in the cache and use that
    if (ncNodeNameCache.keys[lookupId]) {
      return formatLabel(ncNodeNameCache.keys[lookupId], ids.length);
    }

    // No value, so go fetch one
    // We'll put a temp value in the cache though so we don't
    // make a load of requests while we wait for a response
    ncNodeNameCache.keys[lookupId] = "Loading...";

    // If the service has already been invoked, don't do it again
    if (serviceInvoked) {
      return formatLabel(ncNodeNameCache.keys[lookupId], ids.length);
    }

    serviceInvoked = true;

    var udi = udiParser.parse(lookupId);

    if (udi) {
      entityResource.getById(udi.value, udi.entityType).then(function (ent) {
        ncNodeNameCache.keys[lookupId] = ent.name;
      }).catch(function () {
        ncNodeNameCache.keys[lookupId] = "Error: Could not load";
      });
    } else {
      ncNodeNameCache.keys[lookupId] = "Error: Not a UDI";
    }

    // Return the current value for now
    return formatLabel(ncNodeNameCache.keys[lookupId], ids.length);
  }

  return nodeNameFilter;

}).filter("ncRichText", function () {
  return function (input) {
    return $("<div/>").html(input).text();
  };
});
