
function searchResultFormatter(umbRequestHelper) {

    function configureDefaultResult(content, treeAlias, appAlias) {
        content.editorPath = appAlias + "/" + treeAlias + "/edit/" + content.id;
        Utilities.extend(content.metaData, { treeAlias: treeAlias });
    }

    function configureContentResult(content, treeAlias, appAlias) {
        content.menuUrl = umbRequestHelper.getApiUrl("contentTreeBaseUrl", "GetMenu", [{ id: content.id }, { application: appAlias }]);
        content.editorPath = appAlias + "/" + treeAlias + "/edit/" + content.id;
        Utilities.extend(content.metaData, { treeAlias: treeAlias });
        content.subTitle = content.metaData.Url;
    }

    function configureMemberResult(member, treeAlias, appAlias) {
        member.menuUrl = umbRequestHelper.getApiUrl("memberTreeBaseUrl", "GetMenu", [{ id: member.id }, { application: appAlias }]);
        member.editorPath = appAlias + "/" + treeAlias + "/edit/" + (member.key ? member.key : member.id);
        Utilities.extend(member.metaData, { treeAlias: treeAlias });
        member.subTitle = member.metaData.Email;
    }

    function configureMediaResult(media, treeAlias, appAlias) {
        media.menuUrl = umbRequestHelper.getApiUrl("mediaTreeBaseUrl", "GetMenu", [{ id: media.id }, { application: appAlias }]);
        media.editorPath = appAlias + "/" + treeAlias + "/edit/" + media.id;
        Utilities.extend(media.metaData, { treeAlias: treeAlias });
    }

    return {
        configureContentResult: configureContentResult,
        configureMemberResult: configureMemberResult,
        configureMediaResult: configureMediaResult,
        configureDefaultResult: configureDefaultResult
    };
}

angular.module('umbraco.services').factory('searchResultFormatter', searchResultFormatter);
