const _ = require('lodash');
const tough = require('tough-cookie');
const { v4: uuidv4 } = require('uuid');
const tempDataStorage = require("./tempData");

// Export methods for Artillery to be able to use
module.exports = {
    beforeRequest,
    afterResponse,
    beforeScenario,
    afterScenario,
    writeDebug,
    writeError,
    configureNewGuid,
    reportLatency
}

// TODO: If we don't re-use this thing we should just make it specific to doc types instead of generically like this
const guidTypes = {
    "docTypes": /__NEW_DT_GUID__/g
}


/** Reports custom latency statistics */
// TODO: This doesn't work with the latest artillery version but it's cool to see an example of
// how you can get your own custom statistics into artillery.
function reportLatency(req, res, context, events, next) {
    // See https://github.com/artilleryio/artillery/issues/801

    // events.emit('customStat', { stat: `waitTime for ${req.name || req.url}`, value: res.timingPhases.wait });
    // events.emit('customStat', { stat: `dnsTime for ${req.name || req.url}`, value: res.timingPhases.dns });
    // events.emit('customStat', { stat: `tcpTime for ${req.name || req.url}`, value: res.timingPhases.tcp });
    // events.emit('customStat', { stat: `firstByteTime for ${req.name || req.url}`, value: res.timingPhases.firstByte });
    // events.emit('customStat', { stat: `downloadTime for ${req.name || req.url}`, value: res.timingPhases.download });
    // events.emit('customStat', { stat: `totalTime for ${req.name || req.url}`, value: res.timingPhases.total });

    // This was a test and it works
    // events.emit('customStat', { stat: `blah blah`, value: 123 });

    return next();
}

/** Adds the newGuid to the context vars as a new GUID */
function configureNewGuid(requestParams, context, ee, next) {
    context.vars.newGuid = uuidv4();
    return next();
}

function getCookieValueFromResponse(cookieName, response) {
    if (!response.headers || !response.headers['set-cookie']) {
        return null;
    }
    let cookies = response.headers['set-cookie'].map(tough.Cookie.parse);
    var found = _.find(cookies, c => c.key === cookieName);
    return found || null;
}

function setCookieIfMissing(context, requestParams, cookieDictionary) {

    // Hack!
    // This is not required if you specify ANY default cookie value in config, but if you
    // don't and this is disabled then setting a cookie won't work
    // See https://github.com/artilleryio/artillery/issues/1035
    if (!context._enableCookieJar) {
        writeDebug(context, "context._enableCookieJar is disabled, it is being enabled");
        context._enableCookieJar = true;
        requestParams.cookieJar = context._jar;
    }

    // Undocumented - this is a tough-cookie obj instance: https://github.com/salesforce/tough-cookie
    // seen here: https://gitter.im/shoreditch-ops/artillery?at=581bb2912d4796175f3ed473
    let cookieJar = context._jar; // same as requestParams.cookieJar
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
function replaceAndStoreGuid(requestParams, tempData) {

    if (requestParams.body || requestParams.formData) {

        if (!tempData.guids) {
            tempData.guids = {};
        }
        _.forEach(guidTypes, function (guidRegex, guidTypeName) {
            let guid = uuidv4();
            let guidUsed = false;

            if (requestParams.body) {
                requestParams.body = requestParams.body.replace(guidRegex, guid);
                guidUsed = requestParams.body.indexOf(guid) >= 0;
            }

            if (requestParams.formData) {
                _.forEach(requestParams.formData, function (part, partKey) {

                    let replaced = part.replace(guidRegex, guid);
                    requestParams.formData[partKey] = replaced;
                    guidUsed = replaced.indexOf(guid) >= 0;
                    if (guidUsed) {
                        console.log(requestParams.formData[partKey]);
                    }
                });
            }

            if (guidUsed) {
                let guids = [];
                if (tempData.guids[guidTypeName]) {
                    guids = tempData.guids[guidTypeName];
                }
                else {
                    tempData.guids[guidTypeName] = guids;
                }
                guids.push(guid);

                // update/persist this value to temp storage
                tempDataStorage.saveTempData({ guids: tempData.guids });
            }
        });
    }
}

function writeDebug(context, msg) {
    if (context.vars.$processEnvironment.U_DEBUG === 'true') {
        console.log("DEBUG: " + msg);
    }
}

function writeError(msg) {
    console.error("ERROR: ");
    console.error(msg);
}

/** Called when artillery sends a request to set xsrf/cookies */
function beforeRequest(requestParams, context, ee, next) {

    const tempData = tempDataStorage.getTempData();

    replaceAndStoreGuid(requestParams, tempData);

    const isLogin = requestParams.url.endsWith("PostLogin");

    // set the xsrf header if we've captured it
    if (context.vars.umbXsrf) {
        writeDebug(context, "SETTING XSRF HEADER FROM CTX: " + context.vars.umbXsrf.value);
        requestParams.headers["X-UMB-XSRF-TOKEN"] = context.vars.umbXsrf.value;
    }
    else if (!isLogin && tempData.umbXsrf) {
        let val = tough.Cookie.parse(tempData.umbXsrf).value;
        writeDebug(context, "SETTING XSRF HEADER FROM STORAGE: " + val);

        // set from storage
        requestParams.headers["X-UMB-XSRF-TOKEN"] = val;
    }

    // set any missing cookies if we have them stored if it's not the login request
    if (!isLogin) {
        setCookieIfMissing(context, requestParams, {
            "X-UMB-XSRF-TOKEN": tempData.umbXsrf,
            "UMB-XSRF-V": tempData.umbXsrfV,
            "UMB_UCONTEXT": tempData.umbAuthCookie
        });
    }

    return next();
}

/** Called when artillery receives a response to capture the cookies/xsrf */
function afterResponse(requestParams, response, context, ee, next) {

    let acceptedStatusCode = context.acceptedStatusCode || 200;
    if (response.statusCode != acceptedStatusCode) {
        // Kill the process if we don't have a 200 code
        const msg = `Non ${acceptedStatusCode} status code returned: ${response.statusCode}`;
        writeError(msg);
        writeError(response);
        throw msg;
    }

    writeDebug(context, `Status Code: ${response.statusCode}`);

    // If we have a jsonResponse var in the response it means we've flagged
    // the response to be captured
    if ((typeof context.vars.jsonResponse) === 'string') {
        let json = JSON.parse(context.vars.jsonResponse.trim());
        // now set the json object on the context properties to be used in beforeResponse
        context.vars.jsonResponse = json;
    }

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
    tempDataStorage.saveTempData(tempStorage);

    return next();
}

function beforeScenario(context, ee, next) {
    // will occur before each scenario PER user, so this isn't for the entire scenario operation!
    return next();
}

function afterScenario(context, ee, next) {
    // will occur after each scenario PER user, so this isn't for the entire scenario operation!
    return next();
}
