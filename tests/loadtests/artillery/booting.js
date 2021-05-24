// Export methods for Artillery to be able to use
module.exports = {
    storeStatusCode,
    continueUntil200
}

/** Stores the response status code in the context */
function storeStatusCode(requestParams, response, context, ee, next) {
    context.vars.bootResponse = response.statusCode;
    return next();
}

/** Continue until 200 response */
function continueUntil200(context, next) {
    return next(context.vars.bootResponse !== 200);
}
