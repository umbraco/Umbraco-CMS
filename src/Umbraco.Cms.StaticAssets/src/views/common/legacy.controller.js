/**
 * @ngdoc controller
 * @name Umbraco.LegacyController
 * @function
 * 
 * @description
 * A controller to control the legacy iframe injection
 * 
*/
function LegacyController($scope, $routeParams, $element) {

    var url = decodeURIComponent($routeParams.url.replace(/javascript\:/gi, ""));
    //split into path and query
    var urlParts = url.split("?");
    var extIndex = urlParts[0].lastIndexOf(".");
    var ext = extIndex === -1 ? "" : urlParts[0].substr(extIndex);
    //path cannot be a js file
    if (ext !== ".js" || ext === "") {
        //path cannot contain any of these chars
        var toClean = "*(){}[];:<>\\|'\"";
        for (var i = 0; i < toClean.length; i++) {
            var reg = new RegExp("\\" + toClean[i], "g");
            urlParts[0] = urlParts[0].replace(reg, "");
        }
        //join cleaned path and query back together
        url = urlParts[0] + (urlParts.length === 1 ? "" : ("?" + urlParts[1]));
        $scope.legacyPath = url;
    }
    else {
        throw "Invalid url";
    }
}

angular.module("umbraco").controller('Umbraco.LegacyController', LegacyController);