
/*********************************************************************************************************/
/* Canvasdesigner panel app and controller */
/*********************************************************************************************************/

var app = angular.module("Umbraco.canvasdesigner", ['umbraco.resources', 'umbraco.services'])

.controller("Umbraco.canvasdesignerController", function ($scope, $http, $window, $timeout, $location, dialogService) {

    var isInit = $location.search().init;
    if (isInit === "true") {
        //do not continue, this is the first load of this new window, if this is passed in it means it's been
        //initialized by the content editor and then the content editor will actually re-load this window without
        //this flag. This is a required trick to get around chrome popup mgr. We don't want to double load preview.aspx
        //since that will double prepare the preview documents
        return;
    }

    $scope.isOpen = false;
    $scope.frameLoaded = false;
    var pageId = $location.search().id;    
    $scope.pageId = pageId;
    $scope.pageUrl = "frame?id=" + pageId;
    $scope.valueAreLoaded = false;
    $scope.devices = [
        { name: "desktop", css: "desktop", icon: "icon-display", title: "Desktop" },
        { name: "laptop - 1366px", css: "laptop border", icon: "icon-laptop", title: "Laptop" },
        { name: "iPad portrait - 768px", css: "iPad-portrait border", icon: "icon-ipad", title: "Tablet portrait" },
        { name: "iPad landscape - 1024px", css: "iPad-landscape border", icon: "icon-ipad flip", title: "Tablet landscape" },
        { name: "smartphone portrait - 480px", css: "smartphone-portrait border", icon: "icon-iphone", title: "Smartphone portrait" },
        { name: "smartphone landscape  - 320px", css: "smartphone-landscape border", icon: "icon-iphone flip", title: "Smartphone landscape" }
    ];
    $scope.previewDevice = $scope.devices[0];
    
    /*****************************************************************************/
    /* Preview devices */
    /*****************************************************************************/

    // Set preview device
    $scope.updatePreviewDevice = function (device) {
        $scope.previewDevice = device;
    };

    /*****************************************************************************/
    /* Exit Preview */
    /*****************************************************************************/

    $scope.exitPreview = function () {
        window.top.location.href = "../endPreview.aspx?redir=%2f" + $scope.pageId;
    };



    /*****************************************************************************/
    /* Panel managment */
    /*****************************************************************************/

    $scope.openPreviewDevice = function () {
        $scope.showDevicesPreview = true;
        $scope.closeIntelCanvasdesigner();
    }

    /*****************************************************************************/
    /* Call function into the front-end   */
    /*****************************************************************************/
    

    var hideUmbracoPreviewBadge = function () {
        var iframe = (document.getElementById("resultFrame").contentWindow || document.getElementById("resultFrame").contentDocument);
        if(iframe.document.getElementById("umbracoPreviewBadge"))
			iframe.document.getElementById("umbracoPreviewBadge").style.display = "none";
    };
    
    /*****************************************************************************/
    /* Init */
    /*****************************************************************************/
    

    // signalr hub
    var previewHub = $.connection.previewHub;

    previewHub.client.refreshed = function (message, sender) {
        console.log("Notified by SignalR preview hub (" + message+ ").");

        if ($scope.pageId != message) {
            console.log("Not a notification for us (" + $scope.pageId + ").");
            return;
        }

        var resultFrame = document.getElementById("resultFrame");
        var iframe = (resultFrame.contentWindow || resultFrame.contentDocument);
        //setTimeout(function() { iframe.location.reload(); }, 1000);
        iframe.location.reload();
    };

    $.connection.hub.start()
        .done(function () { console.log("Connected to SignalR preview hub (ID=" + $.connection.hub.id + ")"); })
        .fail(function () { console.log("Could not connect to SignalR preview hub."); });
})
    

.directive('iframeIsLoaded', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            element.load(function () {
                var iframe = (element.context.contentWindow || element.context.contentDocument);
                if(iframe && iframe.document.getElementById("umbracoPreviewBadge"))
					iframe.document.getElementById("umbracoPreviewBadge").style.display = "none";
                if (!document.getElementById("resultFrame").contentWindow.refreshLayout) {
                    scope.frameLoaded = true;
                    scope.$apply();
                }
            });
        }
    };
})
