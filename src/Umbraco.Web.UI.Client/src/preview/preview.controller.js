
/*********************************************************************************************************/
/* Preview app and controller */
/*********************************************************************************************************/

var app = angular.module("umbraco.preview", ['umbraco.resources', 'umbraco.services'])

    .controller("previewController", function ($scope, $window, $location) {
        
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

        function configureSignalR(iframe) {
            // signalr hub
            var previewHub = $.connection.previewHub;

            // visibility tracking
            var dirtyContent = false;
            var visibleContent = true;

            document.addEventListener('visibilitychange', function () {
                visibleContent = !document.hidden;
                if (visibleContent && dirtyContent) {
                    dirtyContent = false;
                    console.log("Visible, reloading.")
                    var iframeDoc = (iframe.contentWindow || iframe.contentDocument);
                    iframeDoc.location.reload();
                }
            });

            previewHub.client.refreshed = function (message, sender) {
                console.log("Notified by SignalR preview hub (" + message + ").");

                if ($scope.pageId != message) {
                    console.log("Not a notification for us (" + $scope.pageId + ").");
                    return;
                }

                if (!visibleContent) {
                    console.log("Not visible, will reload.");
                    dirtyContent = true;
                    return;
                }

                console.log("Reloading.");
                var iframeDoc = (iframe.contentWindow || iframe.contentDocument);
                iframeDoc.location.reload();
            };

            $.connection.hub.start()
                .done(function () { console.log("Connected to SignalR preview hub (ID=" + $.connection.hub.id + ")"); })
                .fail(function () { console.log("Could not connect to SignalR preview hub."); });
        }

        var isInit = getParameterByName("init");
        if (isInit === "true") {
            //do not continue, this is the first load of this new window, if this is passed in it means it's been
            //initialized by the content editor and then the content editor will actually re-load this window without
            //this flag. This is a required trick to get around chrome popup mgr. 
            return;
        }

        setPageUrl();

        $scope.isOpen = false;
        $scope.frameLoaded = false;

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

        
        function setPageUrl(){
            $scope.pageId = $location.search().id || getParameterByName("id");
            var culture = $location.search().culture || getParameterByName("culture");

            if ($scope.pageId) {
                var query = 'id=' + $scope.pageId;
                if (culture) {
                    query += "&culture=" + culture;
                }
                $scope.pageUrl = "frame?" + query;
            }
        }
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

            var culture = $location.search().culture || getParameterByName("culture");
            var relativeUrl = "/" +  $scope.pageId +'?culture='+ culture;
            window.top.location.href = "../preview/end?redir=" + encodeURIComponent(relativeUrl);
        };

        $scope.onFrameLoaded = function (iframe) {
            $scope.frameLoaded = true;
            configureSignalR(iframe);
        };

        /*****************************************************************************/
        /* Panel management */
        /*****************************************************************************/

        $scope.openPreviewDevice = function () {
            $scope.showDevicesPreview = true;
        };

        /*****************************************************************************/
        /* Change culture */
        /*****************************************************************************/
        $scope.changeCulture = function (culture) {
          //  $scope.frameLoaded = false;
            $location.search("culture", culture);
            setPageUrl();
        };
        
    })


    .component('previewIFrame', {
        template: "<div style='width:100%;height:100%;margin:0 auto;overflow:hidden;'><iframe id='resultFrame' src='about:blank' ng-src=\"{{vm.src}}\" frameborder='0'></iframe></div>",
        controller: function ($element, $scope, angularHelper) {

            var vm = this;

            vm.$postLink = function () {
                var resultFrame = $element.find("#resultFrame");
                resultFrame.on("load", iframeReady);
            };

            function iframeReady() {
                var iframe = $element.find("#resultFrame").get(0);
                angularHelper.safeApply($scope, function () {
                    vm.onLoaded({ iframe: iframe });
                });
            }


        },
        controllerAs: "vm",
        bindings: {
            src: "<",
            onLoaded: "&"
        }

    })

    .config(function ($locationProvider) {
        $locationProvider.html5Mode(false); //turn html5 mode off
        $locationProvider.hashPrefix('');
    });
