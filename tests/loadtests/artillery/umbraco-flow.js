// Export methods for Artillery to be able to use
module.exports = {
    captureXsrf: captureXsrf,
    sendXsrf: sendXsrf,
    beforeScenario: beforeScenario,
    afterScenario: afterScenario
}

/** Called when artillery sends a request */
function sendXsrf(requestParams, context, ee, next) {
    // set the xsrf header if we've captured it
    if (context.vars.umbXsrf) {
        requestParams.headers["X-UMB-XSRF-TOKEN"] = context.vars.umbXsrf;
    }
    return next();
}

/** Called when artillery receives a response */
function captureXsrf(requestParams, response, context, ee, next) {
    // extract the xsrf value
    if (response.headers && response.headers["set-cookie"]) {
        var cookies = response.headers["set-cookie"]; // a string array
        for (var i = 0; i < cookies.length; i++) {
            var parts = cookies[i].split("=");
            if (parts[0] == "UMB-XSRF-TOKEN") {
                var val = parts[1].split(";")[0].trim();
                context.vars.umbXsrf = val;
                // console.log("XSRF VAL: " + context.vars.umbXsrf);
                break;
            }
        }
    }
    return next();
}

function beforeScenario(context, ee, next) {
    // TODO: We could execute things before/after each scenario
    return next();
}

function afterScenario(context, ee, next) {
    // TODO: We could execute things before/after each scenario
    return next();
}
