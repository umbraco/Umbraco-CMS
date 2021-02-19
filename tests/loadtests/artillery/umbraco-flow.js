// Export methods for Artillery to be able to use
module.exports = {
    beforeRequest: beforeRequest,
    loginAndLoad: loginAndLoad,
    createDocType: createDocType,
    afterScenario: afterScenario,
    afterResponse: afterResponse
}

const fs = require('fs');
const _ = require('lodash');
const tough = require('tough-cookie');
const { v4: uuidv4 } = require('uuid');
const Shell = require('node-powershell');
const ps = new Shell({
    executionPolicy: 'Bypass',
    noProfile: true
});

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
        let guid = uuidv4();
        requestParams.body = requestParams.body.replace(/NEW_GUID/g, guid);

        let guids = [];
        if (tempData.guids) {
            guids = tempData.guids;
        }
        guids.push(guid);

        // update/persist this value to temp storage
        updateTempStorage({ guids: guids });
    }
}

function stopProfilingCounters() {
    let cmd = `
    Write-Verbose "Stopping counters"
    $Job | Receive-Job -Keep # see if there are any errors
    $Batch = Get-Job -Name "MyCounters"
    $Batch | Stop-Job
    $Batch | Remove-Job
    `

    ps.addCommand(cmd);

    return ps.invoke()
        .then(output => {
            console.log(output);
        })
        .catch(err => {
            console.log(err);
        });
}

function startProfilingCounters(name, context) {

    let cmd = `

    $Counters = @(
        "\\\\__U_SERVERNAME__\\Process(__U_PROCESSNAME__)\\% Processor Time",
        # We won't use Private Bytes because this is not compat with dotnet-counters
        #"\\\\__U_SERVERNAME__\\Process(__U_PROCESSNAME__)\\Private Bytes",
        "\\\\__U_SERVERNAME__\\Process(__U_PROCESSNAME__)\\Working Set",
        # We don't care too much about this one, it normally is just small/consistent Gen1+ is the important ones
        "\\\\__U_SERVERNAME__\\.NET CLR Memory(__U_PROCESSNAME__)\\Gen 0 heap size",
        "\\\\__U_SERVERNAME__\\.NET CLR Memory(__U_PROCESSNAME__)\\Gen 1 heap size",
        "\\\\__U_SERVERNAME__\\.NET CLR Memory(__U_PROCESSNAME__)\\Gen 2 heap size",
        "\\\\__U_SERVERNAME__\\.NET CLR Memory(__U_PROCESSNAME__)\\Large Object Heap size",
        # Includes the sum of all managed heaps – Gen 0 + Gen 1 + Gen 2 + LOH. This represents the allocated managed memory size.
        "\\\\__U_SERVERNAME__\\.NET CLR Memory(__U_PROCESSNAME__)\\# Bytes in all Heaps")

    $GetCountersScript = {
        Get-Counter -Counter $args[0] -ComputerName $args[1] -MaxSamples 1000 | Export-Counter -path "$($args[2])" -FileFormat csv -Force
    }
    $Job = Start-Job $GetCountersScript -Name "MyCounters" -ArgumentList $Counters,__U_SERVERNAME__,"__U_SCRIPTROOT__\\output\\__SCENARIO__.csv"
    $Job | Receive-Job -Keep

    `;

    cmd = cmd.replace(/__U_SERVERNAME__/g, context.vars.$processEnvironment.U_SERVERNAME);
    cmd = cmd.replace(/__U_PROCESSNAME__/g, context.vars.$processEnvironment.U_PROCESSNAME);
    cmd = cmd.replace(/__SCENARIO__/g, name);
    cmd = cmd.replace(/__U_SCRIPTROOT__/g, context.vars.$processEnvironment.U_SCRIPTROOT);

    console.log(cmd);

    ps.addCommand(cmd);

    return ps.invoke()
        .then(output => {
            console.log(output);
        })
        .catch(err => {
            console.log(err);
        });
}

/** Called when artillery sends a request to set xsrf/cookies */
function beforeRequest(requestParams, context, ee, next) {
    return loadTempData.then(function () {

        replaceAndStoreGuid(requestParams);

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

/** Called when artillery receives a response to capture the cookies/xsrf */
function afterResponse(requestParams, response, context, ee, next) {

    if (response.statusCode != 200) {
        console.error(response.body);
    }

    return loadTempData.then(function () {

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

function loginAndLoad(context, ee, next) {
    return startProfilingCounters("loginAndLoad", context).then(x => next());
}

function createDocType(context, ee, next) {
    return startProfilingCounters("createDocType", context).then(x => next());
}

function afterScenario(context, ee, next) {
    // TODO: We could execute things before/after each scenario
    // TODO: Would it be possible to notify powershell somehow when this occurs?
    // As it turns out we can run powershell from within nodejs! crazy.
    return stopProfilingCounters().then(x => next());
}
