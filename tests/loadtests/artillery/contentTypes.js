const umbracoFlow = require("./umbracoDataFlow");
const _ = require('lodash');
const tempDataStorage = require("./tempData");

// Export methods for Artillery to be able to use
module.exports = {
    beforeRequest: umbracoFlow.beforeRequest,
    afterResponse: umbracoFlow.afterResponse,
    beforeScenario: umbracoFlow.beforeScenario,
    afterScenario: umbracoFlow.afterScenario,
    configureDocType,
    storeDocTypeId
}

/** Stores the INT doc type ids */
function storeDocTypeId(requestParams, response, context, ee, next) {
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
    if (tempData.ids["docTypes"]) {
        ids = tempData.ids["docTypes"];
    }
    else {
        tempData.ids["docTypes"] = ids;
    }

    ids.push(context.vars.jsonResponse.id);

    // update/persist this value to temp storage
    tempDataStorage.saveTempData({ ids: tempData.ids });

    next();
}

function configureDocType(requestParams, context, ee, next) {
    if (!context.vars.jsonResponse) {
        throw "jsonResponse was not found in the context";
    }
    if ((typeof context.vars.jsonResponse) !== 'object') {
        throw "jsonResponse is not of type 'object'";
    }

    const docTypeProps = ["id", "key", "isContainer", "allowAsRoot", "alias", "description", "thumbnail", "name", "icon", "trashed", "parentId", "path", "allowCultureVariant", "allowSegmentVariant", "isElement"];

    let destDocType = _.pick(context.vars.jsonResponse, docTypeProps);
    destDocType.allowedTemplates = [context.vars.jsonResponse.allowedTemplates[0].alias];
    destDocType.defaultTemplate = destDocType.allowedTemplates[0];
    destDocType.allowedContentTypes = [destDocType.id];
    destDocType.groups = [];
    // need to re-map the properties/groups
    const groupProps = ["inherited", "id", "sortOrder", "name"];
    const propertiesProps = ["id", "alias", "email", "description", "validation", "label", "sortOrder", "dataTypeId", "groupId", "allowCultureVariant", "allowSegmentVariant", "labelOnTop"];
    for (let g = 0; g < context.vars.jsonResponse.groups.length; g++) {
        const sourceGroup = context.vars.jsonResponse.groups[g];
        const destGroup = _.pick(sourceGroup, groupProps);
        destGroup.properties = [];
        for (let p = 0; p < sourceGroup.properties.length; p++) {
            const sourceProp = sourceGroup.properties[p];
            const destProp = _.pick(sourceProp, propertiesProps);
            destGroup.properties.push(destProp);
        }
        destDocType.groups.push(destGroup);
    }

    umbracoFlow.writeDebug(context, requestParams.body);

    // set the request body
    requestParams.body = JSON.stringify(destDocType);

    umbracoFlow.writeDebug(context, requestParams.body);

    next();
}
