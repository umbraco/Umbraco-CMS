const umbracoFlow = require("./umbracoDataFlow");
const _ = require('lodash');
const tempDataStorage = require("./tempData");

// Export methods for Artillery to be able to use
module.exports = {
    beforeRequest: umbracoFlow.beforeRequest,
    afterResponse: umbracoFlow.afterResponse,
    beforeScenario: umbracoFlow.beforeScenario,
    afterScenario: umbracoFlow.afterScenario,
    configureContentTypeAlias,
    configureParentId
}

function configureParentId(requestParams, context, ee, next) {
    if (!context.vars.jsonResponse) {
        throw "jsonResponse was not found in the context";
    }
    if ((typeof context.vars.jsonResponse) !== 'object') {
        throw "jsonResponse is not of type 'object'";
    }

    context.vars.parentId = context.vars.jsonResponse.id;

    next();
}

function configureContentTypeAlias(requestParams, context, ee, next) {

    const tempData = tempDataStorage.getTempData();

    if (!tempData.guids) {
        throw "tempData contains no guids";
    }
    if (!tempData.guids.docTypes) {
        throw "tempData contains no docType guids";
    }

    const contentTypeKey = tempData.guids.docTypes[0];
    const keyCollapsed = contentTypeKey.replace(/\-/g, "");

    // set the variable to be used in the request path
    context.vars.contentTypeAlias = "benchmarkTest" + keyCollapsed;

    next();
}
