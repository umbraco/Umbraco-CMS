// Export methods for Artillery to be able to use
module.exports = {
    storeStatusCode,
    continueUntil200
}

/** Stores the response status code in the context */
function storeStatusCode(req, response, context, ee, next) {
    context.vars.bootResponse = response.statusCode;

    if (response.statusCode === 200) {
        // TODO: We can emit our own timing events
        // to track the specific time of this request

        // console.log(response.timingPhases);

        // ee.emit('bootTime', { stat: `boot time for ${req.name || req.url}`, value: response.timingPhases.total });
    }

    return next();
}

/** Continue until 200 response */
function continueUntil200(context, next) {
    return next(context.vars.bootResponse !== 200);
}
