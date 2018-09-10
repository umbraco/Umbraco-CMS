
/*********************************************************************************************************/
/* Canvasdesigner panel app and controller */
/*********************************************************************************************************/

var app = angular.module("Umbraco.canvasdesigner", ['umbraco.resources', 'umbraco.services'])

    .controller("Umbraco.canvasdesignerController", function ($scope, $http, $window, $timeout, $location, dialogService) {

        //gets a real query string value
        function getParameterByName(name, url) {
            if (!url) url = $window.location.href;
            name = name.replace(/[\[\]]/g, '\\$&');
            var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
                results = regex.exec(url);
            if (!results) return null;
            if (!results[2]) return '';
            return decodeURIComponent(results[2].replace(/\+/g, ' '));
        }

        var isInit = $location.search().init;
        if (isInit === "true") {
            //do not continue, this is the first load of this new window, if this is passed in it means it's been
            //initialized by the content editor and then the content editor will actually re-load this window without
            //this flag. This is a required trick to get around chrome popup mgr. 
            return;
        }

        var pageId = $location.search().id;

        //there is no page id query string hash so check if its part of a 'real' query string
        //and if so, reload with the query string hash
        if (!pageId) {
            var queryStringPageId = getParameterByName("id");
            if (queryStringPageId) {
                $location.search("id", queryStringPageId);
                $window.location.reload();
                return;
            }
        }


        $scope.isOpen = false;
        $scope.frameLoaded = false;

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

        $scope.onFrameLoaded = function () {
            $scope.frameLoaded = true;
        }

        /*****************************************************************************/
        /* Panel managment */
        /*****************************************************************************/

        $scope.openPreviewDevice = function () {
            $scope.showDevicesPreview = true;
            $scope.closeIntelCanvasdesigner();
        }
        
    })


    .component('previewIFrame', {

        template: "<div><iframe id='resultFrame' ng-src=\"{{ vm.src }}\" frameborder='0'></iframe></div>",
        controller: function ($element) {

            var vm = this;

            vm.$onInit = function () {
                
                ////TODO: Move this to the callback on the controller

                //// signalr hub
                //var previewHub = $.connection.previewHub;

                //previewHub.client.refreshed = function (message, sender) {
                //    console.log("Notified by SignalR preview hub (" + message + ").");

                //    if ($scope.pageId != message) {
                //        console.log("Not a notification for us (" + $scope.pageId + ").");
                //        return;
                //    }

                //    var iframe = ($element.context.contentWindow || $element.context.contentDocument);
                //    iframe.location.reload();
                //};

                //$.connection.hub.start()
                //    .done(function () { console.log("Connected to SignalR preview hub (ID=" + $.connection.hub.id + ")"); })
                //    .fail(function () { console.log("Could not connect to SignalR preview hub."); });
            };

            vm.$postLink = function () {
                $element.find("#resultFrame").on("load", function () {
                    var iframe = $element.find("#resultFrame").get(0);
                    hideUmbracoPreviewBadge(iframe);
                    vm.onLoaded();
                    scope.$apply();
                });
            };

            function hideUmbracoPreviewBadge (iframe) {
                if (iframe && iframe.document.getElementById("umbracoPreviewBadge")) {
                    iframe.document.getElementById("umbracoPreviewBadge").style.display = "none";
                }
            };

        },
        controllerAs: "vm",
        bindings: {
            src: "@",
            onLoaded: "&"
        }

    })

    .config(function ($locationProvider) {
        $locationProvider.html5Mode(false); //turn html5 mode off
        $locationProvider.hashPrefix('');
    });
