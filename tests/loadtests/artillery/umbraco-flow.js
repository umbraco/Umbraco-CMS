// Export methods for Artillery to be able to use
module.exports = {
    beforeRequest: beforeRequest,
    afterResponse: afterResponse,
    beforeScenario: beforeScenario,
    afterScenario: afterScenario
}

const fs = require('fs');
const _ = require('lodash');
const tough = require('tough-cookie');
const { v4: uuidv4 } = require('uuid');

const tempDataPath = "output/run.tmp";
let tempData = {};

const loadTempData = new Promise((resolve, reject) => {
    if (fs.existsSync(tempDataPath)) {
        // we use a persisted file to store/share information between
        // this artillery process (node) and the parent powershell process.
        fs.readFile(tempDataPath, function (err, data) {
            if (err) return console.log(err);

            if (data) {
                tempData = JSON.parse(data);
                resolve(true);
            }
        });
    }
    else {
        resolve(true);
    }
});

function updateTempStorage(t) {
    tempData = Object.assign(tempData, t);
    fs.writeFile(tempDataPath, JSON.stringify(tempData, null, 2), function (err) {
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

/** This will check for the special NEW_GUID string in the body and replace with a new GUID and store in persisted storage */
function replaceAndStoreGuid(requestParams) {
    if (requestParams.body) {

        const guidTypes = {
            "docTypes" : /__NEW_DT_GUID__/g
        }

        if (!tempData.guids) {
            tempData.guids = {};
        }
        _.forEach(guidTypes, function (value, key) {

            let guid = uuidv4();
            requestParams.body = requestParams.body.replace(value, guid);

            let guids = [];
            if (tempData.guids[key]) {
                guids = tempData.guids[key];
            }
            else {
                tempData.guids[key] = guids;
            }
            guids.push(guid);

            // update/persist this value to temp storage
            updateTempStorage({ guids: tempData.guids });
        });
    }
}

/** Called when artillery sends a request to set xsrf/cookies */
function beforeRequest(requestParams, context, ee, next) {
    return loadTempData.then(function () {

        replaceAndStoreGuid(requestParams);

        const isLogin = requestParams.url.endsWith("PostLogin");

        // set the xsrf header if we've captured it
        if (context.vars.umbXsrf) {
            console.log("SETTING XSRF HEADER FROM CTX: " + context.vars.umbXsrf.value);
            requestParams.headers["X-UMB-XSRF-TOKEN"] = context.vars.umbXsrf.value;
        }
        else if (!isLogin && tempData.umbXsrf) {
            let val = tough.Cookie.parse(tempData.umbXsrf).value;
            console.log("SETTING XSRF HEADER FROM STORAGE: " + val);

            // set from storage
            requestParams.headers["X-UMB-XSRF-TOKEN"] = val;
        }

        // set any missing cookies if we have them stored if it's not the login request
        if (!isLogin) {
            setCookieIfMissing(context, {
                "X-UMB-XSRF-TOKEN": tempData.umbXsrf,
                "UMB-XSRF-V": tempData.umbXsrfV,
                "UMB_UCONTEXT": tempData.umbAuthCookie
            });
        }

        return next();
    });
}

/** Called when artillery receives a response to capture the cookies/xsrf */
function afterResponse(requestParams, response, context, ee, next) {

    if (response.statusCode != 200) {
        //console.error(response.body);
        console.log(response);
    }

    // TODO: Check if we get jsonResponse here and then can parse?... nope, need to adjust regex
    // Or we always just parse the angular response and always set Id/Key of the object? I dunno it sucks
    // with this angular prefix! There's prob still a way to do it with regex + transform
    console.log(context);
    // const angularPrefix = ")]}',\n";
    // if (response.body.startsWith(angularPrefix)) {
    //     console.log("ANGULAR!");
    //     response.body = response.body.substring(angularPrefix.length);
    // }

    return loadTempData.then(function () {

        let tempStorage = {};
        var xsrf = getCookieValueFromResponse("UMB-XSRF-TOKEN", response);
        if (xsrf) {
            // set this on the context to use later for the xsrf flow.
            // this will be a tough-cookie instance.
            context.vars.umbXsrf = xsrf;
            tempStorage.umbXsrf = xsrf.toString();
        }

        // always save the auth cookie and xsrf-v cookie
        var authCookie = getCookieValueFromResponse("UMB_UCONTEXT", response);
        if (authCookie) {
            // update/persist this value to temp storage
            tempStorage.umbAuthCookie = authCookie.toString();
        }
        var xsrfV = getCookieValueFromResponse("UMB-XSRF-V", response);
        if (xsrfV) {
            // update/persist this value to temp storage
            tempStorage.umbXsrfV = xsrfV.toString();
        }

        // update/persist all updated values to temp storage
        updateTempStorage(tempStorage);

        return next();
    });
}

function beforeScenario(context, ee, next) {
    // will occur after each scenario PER user, so this isn't for the entire scenario operation!
    return next();
}

function afterScenario(context, ee, next) {
    // will occur after each scenario PER user, so this isn't for the entire scenario operation!
    return next();
}
