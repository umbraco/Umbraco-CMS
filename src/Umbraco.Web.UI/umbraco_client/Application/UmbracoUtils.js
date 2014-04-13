/// <reference path="/umbraco_client/Application/NamespaceManager.js" />

Umbraco.Sys.registerNamespace("Umbraco.Utils");

Umbraco.Utils.generateRandom = function() {
    /// <summary>Returns a random integer for use with URLs</summary>
    var day = new Date();
    var z = day.getTime();
    var y = (z - (parseInt(z / 1000, 10) * 1000)) / 10;
    return y;
}