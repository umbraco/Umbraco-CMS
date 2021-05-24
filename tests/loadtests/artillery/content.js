const umbracoFlow = require("./umbracoDataFlow");
const _ = require('lodash');
const tempDataStorage = require("./tempData");

// Export methods for Artillery to be able to use
module.exports = {
    beforeRequest: umbracoFlow.beforeRequest,
    afterResponse: umbracoFlow.afterResponse,
    configureNewGuid: umbracoFlow.configureNewGuid,
    checkDeleteResponse,
    configureContentIdsToDelete,
    configureContentIdsToUpdate,
    configureContentTypeAlias,
    configureParentId,
    deleteUntilNoneLeft,
    storeContentId,
    configureContentIdToDelete,
    configureContentIdToUpdate
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

/** Configures the contentIdsToDelete collection for the scenario */
function configureContentIdsToDelete(context, ee, next) {

    // get all created INT ids and store them into an array in the context
    const tempData = tempDataStorage.getTempData();
    if (tempData.ids && tempData.ids["content"]) {
        context.vars.contentIdsToDelete = tempData.ids["content"];
        // We use this to count how many have been deleted for this scenario,
        // we only want 10 deleted per scenario since that is how many
        // are created per scenario.
        context.vars.contentIdsToDeleteRemaining = 10;
    }
    else {
        // TODO: Throw?
        console.log("no temp data ids!");
        context.vars.contentIdsToDelete = [];
    }

    return next();
}

/** Configures the contentIdsToUpdate collection for the scenario */
function configureContentIdsToUpdate(context, ee, next) {
    // get all created INT ids and store them into an array in the context
    const tempData = tempDataStorage.getTempData();
    if (tempData.ids && tempData.ids["content"]) {
        context.vars.contentIdsToUpdate = tempData.ids["content"];
    }
    else {
        // TODO: Throw?
        console.log("no temp data ids!");
        context.vars.contentIdsToUpdate = [];
    }

    return next();
}

/** Used to configure the INT id for content to delete */
function configureContentIdToDelete(requestParams, context, ee, next) {

    // returns a random integer from 0 to length-1
    let index = Math.floor(Math.random() * context.vars.contentIdsToDelete.length);

    // get/remove the item at the index
    let id = context.vars.contentIdsToDelete.splice(index, 1)[0];

    // store the next content id to delete and remove it from the context
    context.vars.contentIdToDelete = id;

    next();
}

/** Used to check the delete response since we allow 404 (see notes) */
function checkDeleteResponse(requestParams, response, context, ee, next) {
    if (response.statusCode == 404) {
        // In this case it's because the content item has been totally deleted (recycled + deleted).
        // This will occur because each scenario is going to try to delete 10 items
        // in the list and they may be trying to delete the same ones. There's no
        // way to configure this because there's no great way to configure a shared
        // data source between scenarios. We have our persisted file but it's not a great solution.
        // In order to make that work we would need to duplicate all INT ids returned from the
        // create scenario to different arrays: update and delete
        // In that case, we would then remove on INT from that file array for each operation,
        // but it would still be flaky because all of this happens concurrently so there will
        // still be users trying to update/delete the same things.

        context.acceptedStatusCode = 404;
    }
    else if (response.statusCode == 500
        && response.body
        && (response.body.indexOf("FK_umbracoRelation_umbracoNode1") > -1
            || response.body.indexOf("IX_umbracoRelation_parentChildType") > -1)) {
        // This is purely to work around bugs in umbraco. When multiple users are trying to
        // delete items there is a race condition and we'll end up with an exception.
        // This is due to how the RelateOnTrashComponent does data updates which can cause:
        // multiple different issues:
        // * The INSERT statement conflicted with the FOREIGN KEY
        // * Cannot insert duplicate key row in object \'dbo.umbracoRelation\' with unique index \'IX_umbracoRelation_parentChildType\'.
        console.log("Ignoring RelateOnTrashComponent error!");
        context.acceptedStatusCode = 500;
    }
    else {
        context.acceptedStatusCode = 200;

        // Mark one as completed
        context.vars.contentIdsToDeleteRemaining--;
    }

    return next();
}

/** Used for the delete loop until we've deleted 10 or nothing is left */
function deleteUntilNoneLeft(context, next) {

    // if this scenario has processed 10 then stop
    if (context.vars.contentIdsToDeleteRemaining <= 0) {
        return next(false);
    }

    // if there is none left, then stop
    if (context.vars.contentIdsToDelete.length == 0) {
        return next(false);
    }

    return next(true);
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

/** Used to configure the INT id for content to delete, and the content type alias */
function configureContentIdToUpdate(requestParams, context, ee, next) {

    // returns a random integer from 0 to length-1
    let index = Math.floor(Math.random() * context.vars.contentIdsToUpdate.length);

    // get/remove the item at the index
    let id = context.vars.contentIdsToUpdate.splice(index, 1)[0];

    // store the next content id to update and remove it from the context
    context.vars.contentIdToUpdate = id;

    next();
}

/** Sets the content type alias to be used for the content creation, always just uses the first one created */
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
