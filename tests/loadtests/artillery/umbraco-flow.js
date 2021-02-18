// Export methods for Artillery to be able to use
module.exports = {
    sendAuthValues: sendAuthValues,
    beforeScenario: beforeScenario,
    afterScenario: afterScenario,
    captureCookies: captureCookies
}

let fs = require('fs');
let _ = require('lodash');
let tough = require('tough-cookie');

let tempData = null;

const loadTempData = new Promise((resolve, reject) => {
    // we use a persisted file to store/share information between
    // this artillery process (node) and the parent powershell process.
    fs.readFile('output/run.tmp', function (err, data) {
        if (err) return console.log(err);

        if (data) {
            tempData = JSON.parse(data);
            resolve(true);
        }
    });
});

function updateTempStorage(t) {
    if (!tempData) {
        tempData = {};
    }
    tempData = Object.assign(tempData, t);

    fs.writeFile("output/run.tmp", JSON.stringify(tempData, null, 2), function (err) {
        if (err) return console.log(err);
    });
}

function getCookieValueFromResponse(cookieName, response) {
    if (!response.headers || !response.headers['set-cookie']) {
        return null;
    }
    let cookies = response.headers['set-cookie'].map(tough.Cookie.parse);
    var found = _.find(cookies, c => c.key === cookieName);
    return found || null;
}

function setCookieIfMissing(context, cookieDictionary) {

    // Undocumented - this is a tough-cookie obj instance: https://github.com/salesforce/tough-cookie
    // seen here: https://gitter.im/shoreditch-ops/artillery?at=581bb2912d4796175f3ed473
    let cookieJar = context._jar._jar; // same as requestParams.jar._jar._jar
    let cookies = cookieJar.getCookiesSync(context.vars.target);

    _.forEach(cookieDictionary, function (value, key) {

        // if there's a storage value, check if the cookie is missing and if it is, set it as what is stored
        if (value) {
            let cookie = _.find(cookies, x => x.key === key);

            // there isn't a cookie, but it's in our storage, set it
            if (!cookie) {
                cookieJar.setCookieSync(value, context.vars.target);
            }
        }
    });
}

/** Called when artillery sends a request */
function sendAuthValues(requestParams, context, ee, next) {
    loadTempData.then(function () {

        // set the xsrf header if we've captured it
        if (context.vars.umbXsrf) {
            requestParams.headers["X-UMB-XSRF-TOKEN"] = context.vars.umbXsrf.value;
        }
        else if (tempData.umbXsrf) {
            // set from storage
            requestParams.headers["X-UMB-XSRF-TOKEN"] = tough.Cookie.parse(tempData.umbXsrf).value;
        }

        // set any missing cookies if we have them stored if it's not the login request
        if (!requestParams.url.endsWith("PostLogin")) {
            setCookieIfMissing(context, {
                "X-UMB-XSRF-TOKEN": tempData.umbXsrf,
                "UMB-XSRF-V": tempData.umbXsrfV,
                "UMB_UCONTEXT": tempData.umbAuthCookie
            });
        }

        return next();
    });
}

/** Called when artillery receives a response to capture the xsrf */
function captureCookies(requestParams, response, context, ee, next) {

    if (response.statusCode == 500) {
        console.error(response.body);
    }

    loadTempData.then(function () {

        var xsrf = getCookieValueFromResponse("UMB-XSRF-TOKEN", response);
        if (xsrf) {
            // set this on the context to use later for the xsrf flow.
            // this will be a tough-cookie instance.
            context.vars.umbXsrf = xsrf;
            // update/persist this value to temp storage
            updateTempStorage({ umbXsrf: xsrf.toString() });
        }

        // always save the auth cookie and xsrf-v cookie
        var authCookie = getCookieValueFromResponse("UMB_UCONTEXT", response);
        if (authCookie) {
            // update/persist this value to temp storage
            updateTempStorage({ umbAuthCookie: authCookie.toString() });
        }
        var xsrfV = getCookieValueFromResponse("UMB-XSRF-V", response);
        if (xsrfV) {
            // update/persist this value to temp storage
            updateTempStorage({ umbXsrfV: xsrfV.toString() });
        }

        return next();
    });
}

function beforeScenario(context, ee, next) {
    // TODO: We could execute things before/after each scenario
    return next();
}

function afterScenario(context, ee, next) {
    // TODO: We could execute things before/after each scenario
    return next();
}
