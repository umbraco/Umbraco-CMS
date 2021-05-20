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
    configureParentId,
    deleteUntilNoneLeft,
    storeContentId,
    configureContentIdToDelete
}

/** Stores the INT content ids */
function storeContentId(requestParams, response, context, ee, next) {
    if (!context.vars.jsonResponse) {
        throw "jsonResponse was not found in the context";
    }
    if ((typeof context.vars.jsonResponse) !== 'object') {
        throw "jsonResponse is not of type 'object'";
    }

    const tempData = tempDataStorage.getTempData();
    if (!tempData.ids) {
        tempData.ids = {};
    }

    let ids = [];
    if (tempData.ids["content"]) {
        ids = tempData.ids["content"];
    }
    else {
        tempData.ids["content"] = ids;
    }

    ids.push(context.vars.jsonResponse.id);

    // update/persist this value to temp storage
    tempDataStorage.saveTempData({ ids: tempData.ids });

    next();
}

function configureContentIdToDelete(requestParams, context, ee, next) {
    // get all created INT ids and store them into an array in the context
    if (!context.vars.contentIdsToDelete) {
        const tempData = tempDataStorage.getTempData();
        if (tempData.ids && tempData.ids["content"]) {
            context.vars.contentIdsToDelete = tempData.ids["content"];
        }
        else {
            console.log("no temp data ids!");
            context.vars.contentIdsToDelete = [];
        }
    }

    // store the next content id to delete and remove it from the context
    context.vars.contentIdToDelete = context.vars.contentIdsToDelete.pop();

    next();
}

/** Used for the delete loop until nothing is left */
function deleteUntilNoneLeft(context, next) {
    return next(context.vars.contentIdsToDelete.length > 0);
}

/** Sets context.vars.parentId to the root created content item to be used to create children */
function configureParentId(requestParams, context, ee, next) {
    if (!context.vars.parentId) {
        if (!context.vars.jsonResponse) {
            throw "jsonResponse was not found in the context";
        }
        if ((typeof context.vars.jsonResponse) !== 'object') {
            throw "jsonResponse is not of type 'object'";
        }

        context.vars.parentId = context.vars.jsonResponse.id;
    }

    next();
}

/** Sets the content type alias to be used for the content creation */
function configureContentTypeAlias(requestParams, context, ee, next) {

    if (!context.vars.contentTypeAlias) {
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
    }

    next();
}
