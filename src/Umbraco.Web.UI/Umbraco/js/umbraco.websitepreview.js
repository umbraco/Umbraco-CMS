(function () {
    'use strict';
    /*********************************************************************************************************/
    /* website preview */
    /*********************************************************************************************************/
    (function () {
        if (window.location !== window.parent.location) {
            //we are in an iFrame, so lets skip the dialog.
            return;
        }
        var scriptElement = document.currentScript;
        function setCookie(cname, cvalue, exminutes) {
            var d = new Date();
            d.setTime(d.getTime() + exminutes * 60 * 1000);
            document.cookie = cname + '=' + cvalue + ';expires=' + d.toUTCString() + ';path=/';
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
        function beforeUnloadHandler(e) {
            endPreviewSession();
        }
        window.addEventListener('beforeunload', beforeUnloadHandler, false);
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
                // We are good to secretly end preview mode.
                navigator.sendBeacon(scriptElement.getAttribute('data-umbraco-path') + '/preview/end');
            }
        }
        startPreviewSession();
        function endPreviewMode() {
            resetPreviewSessions();
            window.top.location.href = scriptElement.getAttribute('data-umbraco-path') + '/preview/end?redir=' + encodeURIComponent(window.location.pathname + window.location.search);
        }
        function continuePreviewMode(minutsToExpire) {
            setCookie('UMB-WEBSITE-PREVIEW-ACCEPT', 'true', minutsToExpire || 4);
        }
        var user = getCookie('UMB-WEBSITE-PREVIEW-ACCEPT');
        if (user != 'true') {
            askToViewPublishedVersion();
        } else {
            continuePreviewMode();
        }
        function askToViewPublishedVersion() {
            scriptElement.getAttribute('data-umbraco-path');
            var request = new XMLHttpRequest();
            request.open('GET', scriptElement.getAttribute('data-umbraco-path') + '/LocalizedText');
            request.send();
            request.onreadystatechange = function (e) {
                if (request.readyState == 4 && request.status == 200) {
                    var jsonLocalization = JSON.parse(request.responseText);
                    createAskUserAboutVersionDialog(jsonLocalization);
                }
            };
        }
        function createAskUserAboutVersionDialog(jsonLocalization) {
            var localizeVarsFallback = {
                'viewPublishedContentHeadline': 'Preview content?',
                'viewPublishedContentDescription': 'You have ended preview mode, do you want to continue previewing this content?',
                'viewPublishedContentAcceptButton': 'You have ended preview mode, do you want to continue previewing this content?',
                'viewPublishedContentDeclineButton': 'Preview'
            };
            var umbLocalizedVars = jsonLocalization || {};
            umbLocalizedVars.preview = Object.assign(localizeVarsFallback, jsonLocalization.preview);
            // This modal is also used in preview.js
            var modelStyles = '\n            /* Webfont: LatoLatin-Bold */\n            @font-face {\n                font-family: \'Lato\';\n                src: url(\'https://fonts.googleapis.com/css2?family=Lato:wght@700&display=swap\');\n                font-style: normal;\n                font-weight: 700;\n                font-display: swap;\n                text-rendering: optimizeLegibility;\n            }\n\n            .umbraco-preview-dialog {\n                position: fixed;\n                display: flex;\n                align-items: center;\n                justify-content: center;\n                z-index: 99999999;\n                top:0;\n                bottom:0;\n                left:0;\n                right:0;\n                overflow: auto;\n                background-color: rgba(0,0,0,0.6);\n            }\n\n            .umbraco-preview-dialog__modal {\n                background-color: #fff;\n                border-radius: 6px;\n                box-shadow: 0 3px 7px rgba(0,0,0,0.3);\n                margin: auto;\n                padding: 30px 40px;\n                width: 100%;\n                max-width: 540px;\n                font-family: Lato,Helvetica Neue,Helvetica,Arial,sans-serif;\n                font-size: 15px;\n                line-height: 1.5;\n            }\n\n            .umbraco-preview-dialog__headline {\n                font-weight: 700;\n                font-size: 22px;\n                color: #1b264f;\n                margin-top:10px;\n                margin-bottom:20px;\n            }\n            .umbraco-preview-dialog__question {\n                margin-bottom:30px;\n            }\n            .umbraco-preview-dialog__modal > button {\n                display: inline-block;\n                cursor: pointer;\n                padding: 8px 18px;\n                text-align: center;\n                vertical-align: middle;\n                border-radius: 3px;\n                border:none;\n                font-family: inherit;\n                font-weight: 700;\n                font-size: 15px;\n                float:right;\n                margin-left:10px;\n\n                color: #1b264f;\n                background-color: #f6f1ef;\n            }\n            .umbraco-preview-dialog__modal > button:hover {\n                color: #2152a3;\n                background-color: #f6f1ef;\n            }\n            .umbraco-preview-dialog__modal > button.umbraco-preview-dialog__continue {\n                color: #fff;\n                background-color: #2bc37c;\n            }\n            .umbraco-preview-dialog__modal > button.umbraco-preview-dialog__continue:hover {\n                background-color: #39d38b;\n            }\n        ';
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
            modal.innerHTML = '<div class="umbraco-preview-dialog__headline">'.concat(umbLocalizedVars.preview.viewPublishedContentHeadline, '</div>\n            <div class="umbraco-preview-dialog__question">').concat(umbLocalizedVars.preview.viewPublishedContentDescription, '</div>');
            con.appendChild(modal);
            var continueButton = document.createElement('button');
            continueButton.type = 'button';
            continueButton.className = 'umbraco-preview-dialog__continue';
            continueButton.innerHTML = umbLocalizedVars.preview.viewPublishedContentAcceptButton;
            continueButton.addEventListener('click', endPreviewMode);
            modal.appendChild(continueButton);
            var exitButton = document.createElement('button');
            exitButton.type = 'button';
            exitButton.innerHTML = umbLocalizedVars.preview.viewPublishedContentDeclineButton;
            exitButton.addEventListener('click', function () {
                bodyEl.removeChild(fragment);
                continuePreviewMode(5);
            });
            modal.appendChild(exitButton);
            bodyEl.appendChild(fragment);
            continueButton.focus();
        }
    }());
}());