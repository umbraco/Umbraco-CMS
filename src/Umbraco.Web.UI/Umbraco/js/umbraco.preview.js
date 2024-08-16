(function () {
    'use strict';
    /*********************************************************************************************************/
    /* Preview app and controller */
    /*********************************************************************************************************/
    var app = angular.module('umbraco.preview', [
        'umbraco.resources',
        'umbraco.services'
    ]).controller('previewController', function ($scope, $window, $location, $http) {
        $scope.currentCulture = {
            iso: '',
            title: '...',
            icon: 'icon-loading'
        };
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
            window.addEventListener('keydown', handleFirstTab);
        }
        var iframeWrapper = angular.element('#demo-iframe-wrapper');
        var canvasDesignerPanel = angular.element('#canvasdesignerPanel');
        window.addEventListener('keydown', handleFirstTab);
        window.addEventListener('resize', scaleIframeWrapper);
        iframeWrapper.on('transitionend', scaleIframeWrapper);
        function scaleIframeWrapper() {
            if ($scope.previewDevice.name == 'fullsize') {
                // dont scale fullsize preview
                iframeWrapper.css({ 'transform': '' });
            } else {
                var wrapWidth = canvasDesignerPanel.width();
                // width of the wrapper
                var wrapHeight = canvasDesignerPanel.height();
                var childWidth = iframeWrapper.width() + 30;
                // width of child iframe plus some space
                var childHeight = iframeWrapper.height() + 30;
                // child height plus some space
                var wScale = wrapWidth / childWidth;
                var hScale = wrapHeight / childHeight;
                var scale = Math.min(wScale, hScale, 1);
                // get the lowest ratio, but not higher than 1
                iframeWrapper.css({ 'transform': 'scale(' + scale + ')' });    // set scale
            }
        }
        //gets a real query string value
        function getParameterByName(name, url) {
            if (!url)
                url = $window.location.href;
            name = name.replace(/[\[\]]/g, '\\$&');
            var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'), results = regex.exec(url);
            if (!results)
                return null;
            if (!results[2])
                return '';
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
                    console.log('Visible, reloading.');
                    var iframeDoc = iframe.contentWindow || iframe.contentDocument;
                    iframeDoc.location.reload();
                }
            });
            previewHub.client.refreshed = function (message, sender) {
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
            };
            $.connection.hub.start().done(function () {
                console.log('Connected to SignalR preview hub (ID=' + $.connection.hub.id + ')');
            }).fail(function () {
                console.log('Could not connect to SignalR preview hub.');
            });
        }
        function fixExternalLinks(iframe) {
            // Make sure external links don't open inside the iframe
            Array.from(iframe.contentDocument.getElementsByTagName('a')).filter(function (a) {
                return a.hostname !== location.hostname && !a.target;
            }).forEach(function (a) {
                return a.target = '_top';
            });
        }
        var isInit = getParameterByName('init');
        if (isInit === 'true') {
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
            {
                name: 'fullsize',
                css: 'fullsize',
                icon: 'icon-application-window-alt',
                title: 'Fit browser'
            },
            {
                name: 'desktop',
                css: 'desktop shadow',
                icon: 'icon-display',
                title: 'Desktop'
            },
            {
                name: 'laptop - 1366px',
                css: 'laptop shadow',
                icon: 'icon-laptop',
                title: 'Laptop'
            },
            {
                name: 'iPad portrait - 768px',
                css: 'iPad-portrait shadow',
                icon: 'icon-ipad',
                title: 'Tablet portrait'
            },
            {
                name: 'iPad landscape - 1024px',
                css: 'iPad-landscape shadow',
                icon: 'icon-ipad flip',
                title: 'Tablet landscape'
            },
            {
                name: 'smartphone portrait - 480px',
                css: 'smartphone-portrait shadow',
                icon: 'icon-iphone',
                title: 'Smartphone portrait'
            },
            {
                name: 'smartphone landscape  - 320px',
                css: 'smartphone-landscape shadow',
                icon: 'icon-iphone flip',
                title: 'Smartphone landscape'
            }
        ];
        $scope.previewDevice = $scope.devices[0];
        $scope.sizeOpen = false;
        $scope.cultureOpen = false;
        $scope.toggleSizeOpen = function () {
            $scope.sizeOpen = toggleMenu($scope.sizeOpen);
        };
        $scope.toggleCultureOpen = function () {
            $scope.cultureOpen = toggleMenu($scope.cultureOpen);
        };
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
        };
        function windowBlurHandler() {
            closeOthers();
            $scope.$digest();
        }
        window.addEventListener('blur', windowBlurHandler);
        function windowVisibilityHandler(e) {
            var amountOfPreviewSessions = localStorage.getItem('UmbPreviewSessionAmount');
            // When tab is visible again:
            if (document.hidden === false) {
                checkPreviewState();
            }
        }
        document.addEventListener('visibilitychange', windowVisibilityHandler);
        function beforeUnloadHandler(e) {
            endPreviewSession();
        }
        window.addEventListener('beforeunload', beforeUnloadHandler, false);
        $scope.$on('$destroy', function () {
            window.removeEventListener('blur', windowBlurHandler);
            document.removeEventListener('visibilitychange', windowVisibilityHandler);
            window.removeEventListener('beforeunload', beforeUnloadHandler);
        });
        function setPageUrl() {
            $scope.pageId = $location.search().id || getParameterByName('id');
            var culture = $location.search().culture || getParameterByName('culture');
            if ($scope.pageId) {
                var query = 'id=' + $scope.pageId;
                if (culture) {
                    query += '&culture=' + culture;
                }
                $scope.pageUrl = 'frame?' + query;
            }
        }
        function getCookie(cname) {
            var name = cname + '=';
            var ca = document.cookie.split(';');
            for (var i = 0; i < ca.length; i++) {
                var c = ca[i];
                while (c.charAt(0) == ' ') {
                    c = c.substring(1);
                }
                if (c.indexOf(name) == 0) {
                    return c.substring(name.length, c.length);
                }
            }
            return null;
        }
        function setCookie(cname, cvalue, exminutes) {
            var d = new Date();
            d.setTime(d.getTime() + exminutes * 60 * 1000);
            document.cookie = cname + '=' + cvalue + ';expires=' + d.toUTCString() + ';path=/';
        }
        var hasPreviewDialog = false;
        function checkPreviewState() {
            if (getCookie('UMB_PREVIEW') === null) {
                if (hasPreviewDialog === true)
                    return;
                hasPreviewDialog = true;
                // Ask to re-enter preview mode?
                var localizeVarsFallback = {
                    'returnToPreviewHeadline': 'Preview website?',
                    'returnToPreviewDescription': 'You have ended preview mode, do you want to enable it again to view the latest saved version of your website?',
                    'returnToPreviewAcceptButton': 'Preview latest version',
                    'returnToPreviewDeclineButton': 'View published version'
                };
                var umbLocalizedVars = Object.assign(localizeVarsFallback, $window.umbLocalizedVars);
                // This modal is also used in websitepreview.js
                var modelStyles = '\n\n                    /* Webfont: LatoLatin-Bold */\n                    @font-face {\n                        font-family: \'Lato\';\n                        src: url(\'https://fonts.googleapis.com/css2?family=Lato:wght@700&display=swap\');\n                        font-style: normal;\n                        font-weight: 700;\n                        font-display: swap;\n                        text-rendering: optimizeLegibility;\n                    }\n\n                    .umbraco-preview-dialog {\n                        position: fixed;\n                        display: flex;\n                        align-items: center;\n                        justify-content: center;\n                        z-index: 99999999;\n                        top:0;\n                        bottom:0;\n                        left:0;\n                        right:0;\n                        overflow: auto;\n                        background-color: rgba(0,0,0,0.6);\n                    }\n\n                    .umbraco-preview-dialog__modal {\n                        background-color: #fff;\n                        border-radius: 6px;\n                        box-shadow: 0 3px 7px rgba(0,0,0,0.3);\n                        margin: auto;\n                        padding: 30px 40px;\n                        width: 100%;\n                        max-width: 540px;\n                        font-family: Lato,Helvetica Neue,Helvetica,Arial,sans-serif;\n                        font-size: 15px;\n                    }\n\n                    .umbraco-preview-dialog__headline {\n                        font-weight: 700;\n                        font-size: 22px;\n                        color: #1b264f;\n                        margin-top:10px;\n                        margin-bottom:20px;\n                    }\n                    .umbraco-preview-dialog__question {\n                        margin-bottom:30px;\n                    }\n                    .umbraco-preview-dialog__modal > button {\n                        display: inline-block;\n                        cursor: pointer;\n                        padding: 8px 18px;\n                        text-align: center;\n                        vertical-align: middle;\n                        border-radius: 3px;\n                        border:none;\n                        font-family: inherit;\n                        font-weight: 700;\n                        font-size: 15px;\n                        float:right;\n                        margin-left:10px;\n\n                        color: #1b264f;\n                        background-color: #f6f1ef;\n                    }\n                    .umbraco-preview-dialog__modal > button:hover {\n                        color: #2152a3;\n                        background-color: #f6f1ef;\n                    }\n                    .umbraco-preview-dialog__modal > button.umbraco-preview-dialog__continue {\n                        color: #fff;\n                        background-color: #2bc37c;\n                    }\n                    .umbraco-preview-dialog__modal > button.umbraco-preview-dialog__continue:hover {\n                        background-color: #39d38b;\n                    }\n                ';
                var bodyEl = document.getElementsByTagName('BODY')[0];
                var fragment = document.createElement('div');
                var shadowRoot = fragment.attachShadow({ mode: 'open' });
                var style = document.createElement('style');
                style.innerHTML = modelStyles;
                shadowRoot.appendChild(style);
                var con = document.createElement('div');
                con.className = 'umbraco-preview-dialog';
                shadowRoot.appendChild(con);
                var modal = document.createElement('div');
                modal.className = 'umbraco-preview-dialog__modal';
                modal.innerHTML = '<div class="umbraco-preview-dialog__headline">'.concat(umbLocalizedVars.returnToPreviewHeadline, '</div>\n                    <div class="umbraco-preview-dialog__question">').concat(umbLocalizedVars.returnToPreviewDescription, '</div>');
                con.appendChild(modal);
                var declineButton = document.createElement('button');
                declineButton.type = 'button';
                declineButton.innerHTML = umbLocalizedVars.returnToPreviewDeclineButton;
                declineButton.addEventListener('click', function () {
                    bodyEl.removeChild(fragment);
                    $scope.exitPreview();
                    hasPreviewDialog = false;
                });
                modal.appendChild(declineButton);
                var continueButton = document.createElement('button');
                continueButton.type = 'button';
                continueButton.className = 'umbraco-preview-dialog__continue';
                continueButton.innerHTML = umbLocalizedVars.returnToPreviewAcceptButton;
                continueButton.addEventListener('click', function () {
                    bodyEl.removeChild(fragment);
                    reenterPreviewMode();
                    hasPreviewDialog = false;
                });
                modal.appendChild(continueButton);
                bodyEl.appendChild(fragment);
                continueButton.focus();
            }
        }
        function reenterPreviewMode() {
            //Re-enter Preview Mode
            $http({
                method: 'POST',
                url: '../preview/enterPreview',
                params: { id: $scope.pageId }
            });
            startPreviewSession();
        }
        function getPageURL() {
            var culture = $location.search().culture || getParameterByName('culture');
            var relativeUrl = '/' + $scope.pageId;
            if (culture) {
                relativeUrl += '?culture=' + culture;
            }
            return relativeUrl;
        }
        function startPreviewSession() {
            // lets registrer this preview session.
            var amountOfPreviewSessions = Math.max(localStorage.getItem('UmbPreviewSessionAmount') || 0, 0);
            amountOfPreviewSessions++;
            localStorage.setItem('UmbPreviewSessionAmount', amountOfPreviewSessions);
        }
        function resetPreviewSessions() {
            localStorage.setItem('UmbPreviewSessionAmount', 0);
        }
        function endPreviewSession() {
            var amountOfPreviewSessions = localStorage.getItem('UmbPreviewSessionAmount') || 0;
            amountOfPreviewSessions--;
            localStorage.setItem('UmbPreviewSessionAmount', amountOfPreviewSessions);
            if (amountOfPreviewSessions <= 0) {
                // We are good to end preview mode.
                navigator.sendBeacon('../preview/end');
            }
        }
        startPreviewSession();
        /*****************************************************************************/
        /* Preview devices */
        /*****************************************************************************/
        // Set preview device
        $scope.updatePreviewDevice = function (device) {
            $scope.previewDevice = device;
        };
        /*****************************************************************************/
        /* Open website in preview mode */
        /*****************************************************************************/
        $scope.openInBrowser = function () {
            setCookie('UMB-WEBSITE-PREVIEW-ACCEPT', 'true', 5);
            window.open(getPageURL(), '_blank');
        };
        /*****************************************************************************/
        /* Exit Preview */
        /*****************************************************************************/
        $scope.exitPreview = function () {
            resetPreviewSessions();
            window.top.location.href = '../preview/end?redir=' + encodeURIComponent(getPageURL());
        };
        $scope.onFrameLoaded = function (iframe) {
            $scope.frameLoaded = true;
            configureSignalR(iframe);
            fixExternalLinks(iframe);
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
                $location.search('culture', iso);
                setPageUrl();
            }
        };
        $scope.registerCulture = function (iso, title, isDefault) {
            var cultureObject = {
                iso: iso,
                title: title,
                isDefault: isDefault
            };
            cultures.push(cultureObject);
        };
        $scope.$watch('currentCultureIso', function (oldIso, newIso) {
            // if no culture is selected, we will pick the default one:
            if ($scope.currentCultureIso === null) {
                $scope.currentCulture = cultures.find(function (culture) {
                    return culture.isDefault === true;
                });
                return;
            }
            $scope.currentCulture = cultures.find(function (culture) {
                return culture.iso === $scope.currentCultureIso;
            });
        });
    }).component('previewIFrame', {
        template: '<div style=\'width:100%;height:100%;margin:0 auto;overflow:hidden;\'><iframe id=\'resultFrame\' src=\'about:blank\' ng-src="{{vm.src}}" frameborder=\'0\'></iframe></div>',
        controller: function controller($element, $scope, angularHelper) {
            var vm = this;
            vm.$postLink = function () {
                var resultFrame = $element.find('#resultFrame').get(0);
                resultFrame.addEventListener('load', iframeReady);
            };
            function iframeReady() {
                var iframe = $element.find('#resultFrame').get(0);
                hideUmbracoPreviewBadge(iframe);
                angularHelper.safeApply($scope, function () {
                    vm.onLoaded({ iframe: iframe });
                    $scope.frameLoaded = true;
                });
            }
            function hideUmbracoPreviewBadge(iframe) {
                if (iframe && iframe.contentDocument && iframe.contentDocument.getElementById('umbracoPreviewBadge')) {
                    iframe.contentDocument.getElementById('umbracoPreviewBadge').style.display = 'none';
                }
            }
            ;
        },
        controllerAs: 'vm',
        bindings: {
            src: '<',
            onLoaded: '&'
        }
    }).config(function ($locationProvider) {
        $locationProvider.html5Mode(false);
        //turn html5 mode off
        $locationProvider.hashPrefix('');
    });
}());