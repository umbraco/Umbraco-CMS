yepnope.addFilter(function (resourceObj) {
    var url = resourceObj.url.toLowerCase();
    var rnd = ##rnd##;
    var op = "?";
    if(url.indexOf("lib/") === 0 || url.indexOf("js/umbraco.") === 0 || url.indexOf("dependencyhandler.axd") > 0) {
        
        return resourceObj;
    }
    if(url.indexOf("?") > 0){
        op = "&";
    }

    resourceObj.url = resourceObj.url + op + "umb__rnd=" + rnd;
    return resourceObj;
});