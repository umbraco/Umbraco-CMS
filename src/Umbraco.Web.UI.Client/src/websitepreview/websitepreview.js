
/*********************************************************************************************************/
/* website preview */
/*********************************************************************************************************/

(function() {


    if ( window.location !== window.parent.location ) {
        //we are in an iFrame, so lets skip the dialog.
        return;
    }

    const scriptElement = document.currentScript;

    function setCookie(cname, cvalue, exminutes) {
        var d = new Date();
        d.setTime(d.getTime() + (exminutes * 60 * 1000));
        document.cookie = cname + "=" + cvalue + ";expires="+d.toUTCString() + ";path=/";
    }

    function getCookie(cname) {
        var name = cname + "=";
        var ca = document.cookie.split(";");
        for(var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == " ") {
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
    window.addEventListener("beforeunload", beforeUnloadHandler, false);

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

        if(amountOfPreviewSessions <= 0) {
            // We are good to secretly end preview mode.
            navigator.sendBeacon(scriptElement.getAttribute("data-umbraco-path")+"/preview/end");
        }
    }
    startPreviewSession();

    function endPreviewMode() {
        resetPreviewSessions();
        window.top.location.href = scriptElement.getAttribute("data-umbraco-path")+"/preview/end?redir=" + encodeURIComponent(window.location.pathname+window.location.search);
    }
    function continuePreviewMode(minutsToExpire) {
        setCookie("UMB-WEBSITE-PREVIEW-ACCEPT", "true", minutsToExpire || 4);
    }

    var user = getCookie("UMB-WEBSITE-PREVIEW-ACCEPT");
    if (user != "true") {
        askToViewPublishedVersion();
    } else {
        continuePreviewMode();
    }

    function askToViewPublishedVersion() {

        scriptElement.getAttribute("data-umbraco-path");

        const request = new XMLHttpRequest();
        request.open("GET", scriptElement.getAttribute("data-umbraco-path") + "/LocalizedText");
        request.send();

        request.onreadystatechange = (e) => {
            if (request.readyState == 4 && request.status == 200) {
                const jsonLocalization = JSON.parse(request.responseText);
                createAskUserAboutVersionDialog(jsonLocalization);
            }
        }

    }
    function createAskUserAboutVersionDialog(jsonLocalization) {

        const localizeVarsFallback = {
            "viewPublishedContentHeadline": "Preview content?",
            "viewPublishedContentDescription":"You have ended preview mode, do you want to continue previewing this content?",
            "viewPublishedContentAcceptButton":"You have ended preview mode, do you want to continue previewing this content?",
            "viewPublishedContentDeclineButton":"Preview"
        };

        const umbLocalizedVars = jsonLocalization || {};
        umbLocalizedVars.preview = Object.assign(localizeVarsFallback, jsonLocalization.preview);



        // This modal is also used in preview.js
        var modelStyles = `
            /* Webfont: LatoLatin-Bold */
            @font-face {
                font-family: 'Lato';
                src: url('https://fonts.googleapis.com/css2?family=Lato:wght@700&display=swap');
                font-style: normal;
                font-weight: 700;
                font-display: swap;
                text-rendering: optimizeLegibility;
            }

            .umbraco-preview-dialog {
                position: fixed;
                display: flex;
                align-items: center;
                justify-content: center;
                z-index: 99999999;
                top:0;
                bottom:0;
                left:0;
                right:0;
                overflow: auto;
                background-color: rgba(0,0,0,0.6);
            }

            .umbraco-preview-dialog__modal {
                background-color: #fff;
                border-radius: 6px;
                box-shadow: 0 3px 7px rgba(0,0,0,0.3);
                margin: auto;
                padding: 30px 40px;
                width: 100%;
                max-width: 540px;
                font-family: Lato,Helvetica Neue,Helvetica,Arial,sans-serif;
                font-size: 15px;
                line-height: 1.5;
            }

            .umbraco-preview-dialog__headline {
                font-weight: 700;
                font-size: 22px;
                color: #1b264f;
                margin-top:10px;
                margin-bottom:20px;
            }
            .umbraco-preview-dialog__question {
                margin-bottom:30px;
            }
            .umbraco-preview-dialog__modal > button {
                display: inline-block;
                cursor: pointer;
                padding: 8px 18px;
                text-align: center;
                vertical-align: middle;
                border-radius: 3px;
                border:none;
                font-family: inherit;
                font-weight: 700;
                font-size: 15px;
                float:right;
                margin-left:10px;

                color: #1b264f;
                background-color: #f6f1ef;
            }
            .umbraco-preview-dialog__modal > button:hover {
                color: #2152a3;
                background-color: #f6f1ef;
            }
            .umbraco-preview-dialog__modal > button.umbraco-preview-dialog__continue {
                color: #fff;
                background-color: #2bc37c;
            }
            .umbraco-preview-dialog__modal > button.umbraco-preview-dialog__continue:hover {
                background-color: #39d38b;
            }
        `;

        var bodyEl = document.getElementsByTagName("BODY")[0];

        var fragment = document.createElement("div");
        var shadowRoot = fragment.attachShadow({ mode: 'open' });

        var style = document.createElement("style");
        style.innerHTML = modelStyles;
        shadowRoot.appendChild(style);

        var con = document.createElement("div");
        con.className = "umbraco-preview-dialog";
        shadowRoot.appendChild(con);

        var modal = document.createElement("div");
        modal.className = "umbraco-preview-dialog__modal";
        modal.innerHTML = `<div class="umbraco-preview-dialog__headline">${umbLocalizedVars.preview.viewPublishedContentHeadline}</div>
            <div class="umbraco-preview-dialog__question">${umbLocalizedVars.preview.viewPublishedContentDescription}</div>`;
        con.appendChild(modal);

        var continueButton = document.createElement("button");
        continueButton.type = "button";
        continueButton.className = "umbraco-preview-dialog__continue";
        continueButton.innerHTML = umbLocalizedVars.preview.viewPublishedContentAcceptButton;
        continueButton.addEventListener("click", endPreviewMode);
        modal.appendChild(continueButton);

        var exitButton = document.createElement("button");
        exitButton.type = "button";
        exitButton.innerHTML = umbLocalizedVars.preview.viewPublishedContentDeclineButton;
        exitButton.addEventListener("click", function() {
            bodyEl.removeChild(fragment);
            continuePreviewMode(5);
        });
        modal.appendChild(exitButton);

        bodyEl.appendChild(fragment);
        continueButton.focus();
    }

})();
