
/*********************************************************************************************************/
/* website preview controller */
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
        return "";
    }

    function exitPreviewMode() {
        window.top.location.href = scriptElement.getAttribute("data-umbraco-path")+"/preview/end?redir=" + encodeURIComponent(window.location.pathname+window.location.search);
    }
    function continuePreviewMode() {
        setCookie("UMB-WEBSITE-PREVIEW-ACCEPT", "true", 5);
    }

    var user = getCookie("UMB-WEBSITE-PREVIEW-ACCEPT");
    if (user != "true") {

        var modelStyles = `
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
            }

            .umbraco-preview-dialog__headline {
                font-weight: 700;
                font-size: 22px;
                color: #1b264f;
                margin-top:10px;
                margin-bottom:10px;
            }
            .umbraco-preview-dialog__question {
                margin-bottom:30px;
            }
            .umbraco-preview-dialog__modal > button {
                display: inline-block;
                padding: 8px 18px;
                text-align: center;
                vertical-align: middle;
                border-radius: 3px;
                border:none;
                font-weight: 700;
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

        var fragment = document.createDocumentFragment();

        var style = document.createElement("style");
        style.innerHTML = modelStyles;
        fragment.appendChild(style);

        var con = document.createElement("div");
        con.className = "umbraco-preview-dialog";
        fragment.appendChild(con);

        var modal = document.createElement("div");
        modal.className = "umbraco-preview-dialog__modal";
        modal.innerHTML = `<div class="umbraco-preview-dialog__headline">Preview mode</div>
            <div class="umbraco-preview-dialog__question">Do you want to continue viewing latest saved version?</div>`;
        con.appendChild(modal);

        var continueButton = document.createElement("button");
        continueButton.type = "button";
        continueButton.className = "umbraco-preview-dialog__continue";
        continueButton.innerHTML = "Continue";
        continueButton.addEventListener("click", function() {
            bodyEl.removeChild(style);
            bodyEl.removeChild(con);
            continuePreviewMode();
        });
        modal.appendChild(continueButton);

        var exitButton = document.createElement("button");
        exitButton.type = "button";
        exitButton.innerHTML = "Exit preview mode";
        exitButton.addEventListener("click", exitPreviewMode);
        modal.appendChild(exitButton);

        bodyEl.appendChild(fragment);
        continueButton.focus();
    } else {
        continuePreviewMode();
    }

})();
