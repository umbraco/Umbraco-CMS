
/*********************************************************************************************************/
/* Preview app and controller */
/*********************************************************************************************************/

var app = angular.module("umbraco.preview", ['umbraco.resources', 'umbraco.services'])

    .controller("previewController", function ($scope, $window, $location) {

        $scope.currentCulture = { iso: '', title: '...', icon: 'icon-loading' }
        var cultures = [];

        $scope.tabbingActive = false;
        // There are a number of ways to detect when a focus state should be shown when using the tab key and this seems to be the simplest solution.
        // For more information about this approach, see https://hackernoon.com/removing-that-ugly-focus-ring-and-keeping-it-too-6c8727fefcd2
        function handleFirstTab(evt) {
            if (evt.keyCode === 9) {
                $scope.tabbingActive = true;
                $scope.$digest();
                window.removeEventListener('keydown', handleFirstTab);
                window.addEventListener('mousedown', disableTabbingActive);
            }
        }

        function disableTabbingActive(evt) {
            $scope.tabbingActive = false;
            $scope.$digest();
            window.removeEventListener('mousedown', disableTabbingActive);
            window.addEventListener("keydown", handleFirstTab);
        }

        window.addEventListener("keydown", handleFirstTab);


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
            var previewHub = new signalR.HubConnectionBuilder()
                .withUrl("/umbraco/previewHub")
                .build();

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

            previewHub.on("refreshed", function (message) {
                console.log('Notified by SignalR preview hub (' + message + ').');
                if ($scope.pageId != message) {
                    console.log('Not a notification for us (' + $scope.pageId + ').');
                    return;
                }
                if (!visibleContent) {
                    console.log('Not visible, will reload.');
                    dirtyContent = true;
                    return;
                }
                console.log('Reloading.');
                var iframeDoc = iframe.contentWindow || iframe.contentDocument;
                iframeDoc.location.reload();
            })

            try {
                previewHub.start().then(function () {
                    // console.log('Connected to SignalR preview hub (ID=' + $.connection.hub.id + ')');
                    console.log('SignalR is ' + previewHub.state);
                }).catch(function () {
                    console.log('Could not connect to SignalR preview hub.');
                });
            } catch (e) {
                console.error('Could not establish signalr connection. Error: ' + e);
            }
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
            { name: "fullsize", css: "fullsize", icon: "icon-application-window-alt", title: "Browser" },
            { name: "desktop", css: "desktop shadow", icon: "icon-display", title: "Desktop" },
            { name: "laptop - 1366px", css: "laptop shadow", icon: "icon-laptop", title: "Laptop" },
            { name: "iPad portrait - 768px", css: "iPad-portrait shadow", icon: "icon-ipad", title: "Tablet portrait" },
            { name: "iPad landscape - 1024px", css: "iPad-landscape shadow", icon: "icon-ipad flip", title: "Tablet landscape" },
            { name: "smartphone portrait - 480px", css: "smartphone-portrait shadow", icon: "icon-iphone", title: "Smartphone portrait" },
            { name: "smartphone landscape  - 320px", css: "smartphone-landscape shadow", icon: "icon-iphone flip", title: "Smartphone landscape" }
        ];
        $scope.previewDevice = $scope.devices[0];

        $scope.sizeOpen = false;
        $scope.cultureOpen = false;

        $scope.toggleSizeOpen = function () {
            $scope.sizeOpen = toggleMenu($scope.sizeOpen);
        }
        $scope.toggleCultureOpen = function () {
            $scope.cultureOpen = toggleMenu($scope.cultureOpen);
        }

        function toggleMenu(isCurrentlyOpen) {
            if (isCurrentlyOpen === false) {
                closeOthers();
                return true;
            } else {
                return false;
            }
        }
        function closeOthers() {
            $scope.sizeOpen = false;
            $scope.cultureOpen = false;
        }

        $scope.windowClickHandler = function () {
            closeOthers();
        }
        function windowBlurHandler() {
            closeOthers();
            $scope.$digest();
        }

        var win = $($window);

        win.on("blur", windowBlurHandler);

        $scope.$on("$destroy", function () {
            win.off("blur", handleBlwindowBlurHandlerur);
        });


        function setPageUrl() {
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
            var relativeUrl = "/" + $scope.pageId;
            if (culture) {
                relativeUrl += '?culture=' + culture;
            }
            window.top.location.href = "../preview/end?redir=" + encodeURIComponent(relativeUrl);
        };

        $scope.onFrameLoaded = function (iframe) {
            $scope.frameLoaded = true;
            configureSignalR(iframe);

            $scope.currentCultureIso = $location.search().culture || null;
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
        $scope.changeCulture = function (iso) {
            if ($location.search().culture !== iso) {
                $scope.frameLoaded = false;
                $scope.currentCultureIso = iso;
                $location.search("culture", iso);
                setPageUrl();
            }
        };
        $scope.registerCulture = function (iso, title, isDefault) {
            var cultureObject = { iso: iso, title: title, isDefault: isDefault };
            cultures.push(cultureObject);
        }

        $scope.$watch("currentCultureIso", function (oldIso, newIso) {
            // if no culture is selected, we will pick the default one:
            if ($scope.currentCultureIso === null) {
                $scope.currentCulture = cultures.find(function (culture) {
                    return culture.isDefault === true;
                })
                return;
            }
            $scope.currentCulture = cultures.find(function (culture) {
                return culture.iso === $scope.currentCultureIso;
            })
        });

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
                hideUmbracoPreviewBadge(iframe);
                angularHelper.safeApply($scope, function () {
                    vm.onLoaded({ iframe: iframe });
                    $scope.frameLoaded = true;
                });
            }

            function hideUmbracoPreviewBadge(iframe) {
                if (iframe && iframe.contentDocument && iframe.contentDocument.getElementById("umbracoPreviewBadge")) {
                    iframe.contentDocument.getElementById("umbracoPreviewBadge").style.display = "none";
                }
            };


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
